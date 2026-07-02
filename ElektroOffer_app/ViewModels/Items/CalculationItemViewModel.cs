using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;

namespace ElektroOffer_app.ViewModels.Items
{

    // =========================================================
    // 🧮 CALCULATION ITEM VIEWMODEL
    // =========================================================
    // 👉 ViewModel pro jeden řádek kalkulace v sekci PRÁCE
    // 👉 Řídí kaskádu ComboBoxů (Task → Specification → Material → Location)
    // 👉 Po výběru všech hodnot spočítá celkovou cenu (Total)
    // =========================================================
    public class CalculationItemViewModel : INotifyPropertyChanged
    {
        private PriceItems? _workItem;
        private Material? _materialItem;
        private double _quantity;

        private string? _selectedTask;
        private string? _selectedSpecification;
        private string? _selectedMaterial;
        private string? _selectedLocation;

        private string? _workUnit;

        // =========================================================
        // 💰 SLEVA
        // =========================================================
        // 👉 IsDiscountEnabled – přepíná aktivaci slevy
        //    při vypnutí automaticky vynuluje DiscountPercent
        //    a vyvolá přepočet Total
        // 👉 DiscountPercent – procentuální hodnota slevy
        //    null = sleva není zadána
        //    změna vždy vyvolá přepočet Total
        // =========================================================

        private bool _isDiscountEnabled;

        /// <summary>
        /// Přepínač aktivace slevy na tomto řádku.
        /// Při vypnutí (false) automaticky vynuluje DiscountPercent → null.
        /// Změna vždy vyvolá přepočet Total v UI.
        /// </summary>
        public bool IsDiscountEnabled
        {
            get => _isDiscountEnabled;
            set
            {
                if (_isDiscountEnabled == value) return;
                _isDiscountEnabled = value;

                // Vypnutí slevy → vynuluj procento
                // Píšeme přímo do _discountPercent (privátní field),
                // aby nedošlo ke zbytečnému dvojímu volání OnPropertyChanged
                if (!value)
                {
                    _discountPercent = null;
                    OnPropertyChanged(nameof(DiscountPercent));
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(Total)); // ← přepočet ceny v UI
            }
        }

        private double? _discountPercent;

        /// <summary>
        /// Procentuální výše slevy (0–100).
        /// Null = sleva není zadána.
        /// Změna vždy vyvolá přepočet Total v UI.
        /// </summary>
        public double? DiscountPercent
        {
            get => _discountPercent;
            set
            {
                if (_discountPercent == value) return;
                _discountPercent = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(Total)); // ← přepočet ceny v UI
            }
        }

        // =========================================================
        // 📦 DYNAMICKÉ LISTY PRO COMBOBOXY
        // =========================================================
        public ObservableCollection<string> AvailableSpecifications { get; } = new();
        public ObservableCollection<string> AvailableMaterials { get; } = new();
        public ObservableCollection<string> AvailableLocations { get; } = new();

        // =========================================================
        // 📏 UNIT (MĚRNÁ JEDNOTKA PRÁCE)
        // =========================================================
        public string? WorkUnit
        {
            get => _workUnit;
            set
            {
                if (_workUnit == value) return;
                _workUnit = value;
                OnPropertyChanged();
            }
        }

        // =========================================================
        // 🔒 KASKÁDA POVOLENÍ
        // =========================================================
        // 👉 Určuje, zda jsou jednotlivé ComboBoxy povolené
        // 👉 Závisí na předchozích výběrech
        // =========================================================
        public bool CanSelectSpecification => !string.IsNullOrWhiteSpace(SelectedTask);
        public bool CanSelectMaterial => !string.IsNullOrWhiteSpace(SelectedSpecification);
        public bool CanSelectLocation => !string.IsNullOrWhiteSpace(SelectedMaterial);

        // =========================================================
        // TASK
        // =========================================================
        public string? SelectedTask
        {
            get => _selectedTask;
            set
            {
                if (_selectedTask == value) return;
                _selectedTask = value;

                // Reset kaskády níže + načtení nových hodnot
                ResetBelowTask();
                LoadSpecifications();

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSelectSpecification));
            }
        }

        // =========================================================
        // SPECIFICATION
        // =========================================================
        public string? SelectedSpecification
        {
            get => _selectedSpecification;
            set
            {
                if (_selectedSpecification == value) return;
                _selectedSpecification = value;

                ResetBelowSpecification();
                LoadWorkUnit();
                LoadMaterials();

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSelectMaterial));
            }
        }

        // =========================================================
        // MATERIAL
        // =========================================================
        public string? SelectedMaterial
        {
            get => _selectedMaterial;
            set
            {
                if (_selectedMaterial == value) return;
                _selectedMaterial = value;

                ResetBelowMaterial();
                LoadLocations();

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSelectLocation));
            }
        }

        // =========================================================
        // LOCATION
        // =========================================================
        public string? SelectedLocation
        {
            get => _selectedLocation;
            set
            {
                if (_selectedLocation == value) return;
                _selectedLocation = value;

                UpdateWorkItem();

                OnPropertyChanged();
            }
        }

        // =========================================================
        // WORK ITEM (ZÁZNAM Z TABULKY PRICEITEMS)
        // =========================================================
        public PriceItems? WorkItem
        {
            get => _workItem;
            set
            {
                _workItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        // =========================================================
        // MATERIAL ITEM (ZÁZNAM Z TABULKY MATERIALS)
        // =========================================================
        public Material? MaterialItem
        {
            get => _materialItem;
            set
            {
                _materialItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        // =========================================================
        // QUANTITY (MNOŽSTVÍ)
        // =========================================================
        public double Quantity
        {
            get => _quantity;
            set
            {
                if (Math.Abs(_quantity - value) < 0.0001) return;
                _quantity = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        // =========================================================
        // TOTAL (CELKOVÁ CENA ŘÁDKU + SLEVA)
        // =========================================================
        // 👉 Pokud je vybraná práce → BasePrice * MaterialCoef * PositionCoef * Quantity
        // 👉 Pokud je vybraný materiál → Material.Price * Quantity
        // 👉 Jinak 0
        // 👉 Příklad: 150 Kč × 1,2 × 1,0 × 10 m = 1 800 Kč
        // 👉 Sleva se aplikuje na výsledek pokud IsDiscountEnabled == true
        //    a DiscountPercent má hodnotu
        // =========================================================
        public double Total
        {
            get
            {
                double baseTotal;

                if (WorkItem != null)
                    baseTotal = WorkItem.BasePrice * WorkItem.MaterialCoef * WorkItem.PositionCoef * Quantity;

                else if (MaterialItem != null)
                    baseTotal = MaterialItem.Price * Quantity;

                else
                    return 0;

                // =========================
                // 💰 APLIKACE SLEVY
                // =========================
                // Sleva se aplikuje pouze pokud:
                //   1. IsDiscountEnabled == true  (přepínač je zapnutý)
                //   2. DiscountPercent má hodnotu  (není null)
                // Vzorec: baseTotal × (1 - procento / 100)
                // Příklad: 1 000 Kč × (1 - 10/100) = 1 000 × 0,9 = 900 Kč
                if (IsDiscountEnabled && DiscountPercent.HasValue)
                {
                    return baseTotal * (1 - DiscountPercent.Value / 100.0);
                }

                return baseTotal;
            }
        }

        // =========================================================
        // 📏 UNIT LOAD
        // =========================================================
        // 👉 Načte měrnou jednotku z tabulky PriceItems podle Specification
        // =========================================================
        private void LoadWorkUnit()
        {
            using var db = new AppDbContext();

            WorkUnit = db.PriceItems
                .Where(x => x.Specification == SelectedSpecification)
                .Select(x => x.Unit)
                .FirstOrDefault();
        }

        // =========================================================
        // 🔽 FILTER 1: SPECIFICATIONS
        // =========================================================
        // 👉 Načte seznam specifikací pro vybraný Task
        // =========================================================
        private void LoadSpecifications()
        {
            AvailableSpecifications.Clear();

            using var db = new AppDbContext();

            var list = db.PriceItems
                .Where(x => x.Task == SelectedTask)
                .Select(x => x.Specification)
                .Distinct()
                .ToList();

            foreach (var item in list)
                AvailableSpecifications.Add(item);
        }

        // =========================================================
        // 🔽 FILTER 2: MATERIALS
        // =========================================================
        // 👉 Načte seznam materiálů pro vybraný Task + Specification
        // =========================================================
        private void LoadMaterials()
        {
            AvailableMaterials.Clear();

            using var db = new AppDbContext();

            var list = db.PriceItems
                .Where(x => x.Task == SelectedTask &&
                            x.Specification == SelectedSpecification)
                .Select(x => x.Material)
                .Distinct()
                .ToList();

            foreach (var item in list)
                AvailableMaterials.Add(item);
        }

        // =========================================================
        // 🔽 FILTER 3: LOCATIONS
        // =========================================================
        // 👉 Načte seznam umístění pro vybraný Task + Specification + Material
        // =========================================================
        private void LoadLocations()
        {
            AvailableLocations.Clear();

            using var db = new AppDbContext();

            var list = db.PriceItems
                .Where(x => x.Task == SelectedTask &&
                            x.Specification == SelectedSpecification &&
                            x.Material == SelectedMaterial)
                .Select(x => x.Location)
                .Distinct()
                .ToList();

            foreach (var item in list)
                AvailableLocations.Add(item);
        }

        // =========================================================
        // UPDATE RESULT (NAČTENÍ KONKRÉTNÍHO ZÁZNAMU PRÁCE)
        // =========================================================
        private void UpdateWorkItem()
        {
            if (string.IsNullOrWhiteSpace(SelectedTask) ||
                string.IsNullOrWhiteSpace(SelectedSpecification) ||
                string.IsNullOrWhiteSpace(SelectedMaterial) ||
                string.IsNullOrWhiteSpace(SelectedLocation))
            {
                WorkItem = null;
                return;
            }

            using var db = new AppDbContext();

            WorkItem = db.PriceItems
                .FirstOrDefault(x =>
                    x.Task == SelectedTask &&
                    x.Specification == SelectedSpecification &&
                    x.Material == SelectedMaterial &&
                    x.Location == SelectedLocation);
        }

        // =========================================================
        // RESETY KASKÁDY
        // =========================================================
        // 👉 Používají privátní fieldy přímo → nezpůsobí rekurzivní kaskádu
        // 👉 Kolekce se čistí ručně, notify se posílá cíleně
        // =========================================================
        private void ResetBelowTask()
        {
            _selectedSpecification = null;
            _selectedMaterial = null;
            _selectedLocation = null;

            AvailableSpecifications.Clear();
            AvailableMaterials.Clear();
            AvailableLocations.Clear();

            WorkItem = null;
            WorkUnit = null;

            OnPropertyChanged(nameof(SelectedSpecification));
            OnPropertyChanged(nameof(SelectedMaterial));
            OnPropertyChanged(nameof(SelectedLocation));
            OnPropertyChanged(nameof(CanSelectMaterial));
            OnPropertyChanged(nameof(CanSelectLocation));
        }

        private void ResetBelowSpecification()
        {
            _selectedMaterial = null;
            _selectedLocation = null;

            AvailableMaterials.Clear();
            AvailableLocations.Clear();

            WorkItem = null;

            OnPropertyChanged(nameof(SelectedMaterial));
            OnPropertyChanged(nameof(SelectedLocation));
            OnPropertyChanged(nameof(CanSelectLocation));
        }

        private void ResetBelowMaterial()
        {
            _selectedLocation = null;

            AvailableLocations.Clear();

            WorkItem = null;

            OnPropertyChanged(nameof(SelectedLocation));
        }

        // =========================================================
        // 🔔 NOTIFY
        // =========================================================
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}