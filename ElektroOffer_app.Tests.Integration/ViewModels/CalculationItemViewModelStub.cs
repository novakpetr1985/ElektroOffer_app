using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;

namespace ElektroOffer_app.Tests.Integration.Stubs
{
    public class CalculationItemViewModelStub : INotifyPropertyChanged
    {
        // =========================================================
        // 🔧 DŮLEŽITÉ: DbContext musí být předán zvenčí
        // =========================================================
        // Aplikace dostane defaultní AppDbContext (elektrooffer.db)
        // Testy dostanou SQLite InMemory AppDbContext
        // → díky tomu testy fungují a VM používá správná data
        private readonly AppDbContext _db;

        /// <summary>
        /// Konstruktor pro aplikaci – používá defaultní databázi.
        /// </summary>
        public CalculationItemViewModelStub() : this(new AppDbContext())
        {
        }

        /// <summary>
        /// Konstruktor pro testy – umožňuje předat vlastní InMemory DB.
        /// </summary>
        public CalculationItemViewModelStub(AppDbContext db)
        {
            _db = db;
        }

        // =========================================================
        // 🔧 POLE A PROPERTIES (beze změny)
        // =========================================================

        private PriceItems? _workItem;
        private Material? _materialItem;
        private double _quantity;

        private string? _selectedTask;
        private string? _selectedSpecification;
        private string? _selectedMaterial;
        private string? _selectedLocation;

        private string? _workUnit;

        private bool _isDiscountEnabled;
        private double? _discountPercent;

        public bool IsDiscountEnabled
        {
            get => _isDiscountEnabled;
            set
            {
                if (_isDiscountEnabled == value) return;
                _isDiscountEnabled = value;

                if (!value)
                {
                    _discountPercent = null;
                    OnPropertyChanged(nameof(DiscountPercent));
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        public double? DiscountPercent
        {
            get => _discountPercent;
            set
            {
                if (_discountPercent == value) return;
                _discountPercent = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        public ObservableCollection<string> AvailableSpecifications { get; } = new();
        public ObservableCollection<string> AvailableMaterials { get; } = new();
        public ObservableCollection<string> AvailableLocations { get; } = new();

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

        public bool CanSelectWorkSpecification => !string.IsNullOrWhiteSpace(SelectedTask);
        public bool CanSelectBaseMaterial => !string.IsNullOrWhiteSpace(SelectedSpecification);
        public bool CanSelectPosition => !string.IsNullOrWhiteSpace(SelectedMaterial);

        public string? SelectedTask
        {
            get => _selectedTask;
            set
            {
                if (_selectedTask == value) return;
                _selectedTask = value;

                ResetBelowTask();
                LoadSpecifications();

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSelectWorkSpecification));
            }
        }

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
                OnPropertyChanged(nameof(CanSelectBaseMaterial));
            }
        }

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
                OnPropertyChanged(nameof(CanSelectPosition));
            }
        }

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

                if (IsDiscountEnabled && DiscountPercent.HasValue)
                    return baseTotal * (1 - DiscountPercent.Value / 100.0);

                return baseTotal;
            }
        }

        // =========================================================
        // 🔧 OPRAVENÉ METODY – používají _db místo new AppDbContext()
        // =========================================================

        private void LoadWorkUnit()
        {
            WorkUnit = _db.PriceItems
                .Where(x => x.Specification == SelectedSpecification)
                .Select(x => x.Unit)
                .FirstOrDefault();
        }

        private void LoadSpecifications()
        {
            AvailableSpecifications.Clear();

            var list = _db.PriceItems
                .Where(x => x.Task == SelectedTask)
                .Select(x => x.Specification)
                .Distinct()
                .ToList();

            foreach (var item in list)
                AvailableSpecifications.Add(item);
        }

        private void LoadMaterials()
        {
            AvailableMaterials.Clear();

            var list = _db.PriceItems
                .Where(x => x.Task == SelectedTask &&
                            x.Specification == SelectedSpecification)
                .Select(x => x.Material)
                .Distinct()
                .ToList();

            foreach (var item in list)
                AvailableMaterials.Add(item);
        }

        private void LoadLocations()
        {
            AvailableLocations.Clear();

            var list = _db.PriceItems
                .Where(x => x.Task == SelectedTask &&
                            x.Specification == SelectedSpecification &&
                            x.Material == SelectedMaterial)
                .Select(x => x.Location)
                .Distinct()
                .ToList();

            foreach (var item in list)
                AvailableLocations.Add(item);
        }

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

            WorkItem = _db.PriceItems
                .FirstOrDefault(x =>
                    x.Task == SelectedTask &&
                    x.Specification == SelectedSpecification &&
                    x.Material == SelectedMaterial &&
                    x.Location == SelectedLocation);
        }

        // =========================================================
        // 🔧 RESETY (beze změny)
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
            OnPropertyChanged(nameof(CanSelectBaseMaterial));
            OnPropertyChanged(nameof(CanSelectPosition));
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
            OnPropertyChanged(nameof(CanSelectPosition));
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
