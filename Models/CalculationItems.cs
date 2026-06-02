using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;

namespace ElektroOffer_app
{
    // =========================================================
    // 🧮 KALKULAČNÍ ŘÁDEK (VIEWMODEL PRO 1 ŘÁDEK TABULKY)
    // =========================================================
    public class CalculationItems : INotifyPropertyChanged
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
        // 📦 DYNAMICKÉ LISTY PRO COMBOBOXY
        // =========================================================
        public ObservableCollection<string> AvailableSpecifications { get; set; } = new();
        public ObservableCollection<string> AvailableMaterials { get; set; } = new();
        public ObservableCollection<string> AvailableLocations { get; set; } = new();

        // =========================================================
        // 📏 UNIT
        // =========================================================
        public string? WorkUnit
        {
            get => _workUnit;
            set { _workUnit = value; OnPropertyChanged(); }
        }

        // =========================================================
        // 🔒 KASKÁDA POVOLENÍ
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
                LoadWorkUnit();
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
        // WORK ITEM
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
        // MATERIAL ITEM
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
        // QUANTITY
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
        // TOTAL
        // =========================================================
        public double Total
        {
            get
            {
                if (WorkItem != null)
                    return WorkItem.BasePrice * WorkItem.MaterialCoef * WorkItem.PositionCoef * Quantity;

                if (MaterialItem != null)
                    return MaterialItem.Price * Quantity;

                return 0;
            }
        }

        // =========================================================
        // 📏 UNIT LOAD
        // =========================================================
        private void LoadWorkUnit()
        {
            using var db = new AppDbContext();

            WorkUnit = db.PriceItems
                .Where(x => x.Task == SelectedTask)
                .Select(x => x.Unit)
                .FirstOrDefault();
        }

        // =========================================================
        // 🔽 FILTER 1: SPECIFICATIONS
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
        // UPDATE RESULT
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
        // RESETY
        // =========================================================
        // 👉 Používají privátní fieldy přímo → nezpůsobí rekurzivní kaskádu
        // 👉 Kolekce se čistí ručně, notify se posílá cíleně

        private void ResetBelowTask()
        {
            // Resetujeme fieldy přímo (ne přes settery) a pak ručně notifikujeme
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
        // NOTIFY
        // =========================================================
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
