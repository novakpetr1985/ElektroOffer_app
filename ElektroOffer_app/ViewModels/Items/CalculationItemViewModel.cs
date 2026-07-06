using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.Services;

namespace ElektroOffer_app.ViewModels.Items
{
    public class CalculationItemViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _db;
        private readonly CalculationCascadeService _cascade;
        private readonly CalculationPriceService _price;

        public CalculationItemViewModel(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _cascade = new CalculationCascadeService(db);
            _price = new CalculationPriceService();
        }

        public static CalculationItemViewModel CreateDefault()
            => new CalculationItemViewModel(new AppDbContext());

        // =========================================================
        // FIELDS
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

        // =========================================================
        // PUBLIC COLLECTIONS (UI)
        // =========================================================

        public ObservableCollection<string> AvailableSpecifications { get; } = new();
        public ObservableCollection<string> AvailableMaterials { get; } = new();
        public ObservableCollection<string> AvailableLocations { get; } = new();

        // =========================================================
        // DISCOUNT
        // =========================================================

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

        // =========================================================
        // UNIT
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
        // CASCADE ENABLE FLAGS
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

                _cascade.ResetBelowTask(this);
                _cascade.LoadSpecifications(this);

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

                _cascade.ResetBelowSpecification(this);
                _cascade.LoadWorkUnit(this);
                _cascade.LoadMaterials(this);

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

                _cascade.ResetBelowMaterial(this);
                _cascade.LoadLocations(this);

                SelectedLocation = null;

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

                _cascade.UpdateWorkItem(this);

                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        // =========================================================
        // WORK ITEM / MATERIAL ITEM
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
        // TOTAL (delegováno do služby)
        // =========================================================

        public double Total => _price.CalculateTotal(this);

        // =========================================================
        // NOTIFY
        // =========================================================

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
