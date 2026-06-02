using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using ElektroOffer_app.Models;
using ElektroOffer_app.Data;

namespace ElektroOffer_app
{
    // =========================================================
    // 🧮 KALKULAČNÍ ŘÁDEK
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

        // =========================================================
        // 📏 UNIT PRO WORK (NOVÉ)
        // =========================================================
        private string? _workUnit;

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
        // 🔒 LOCKY PRO KASKÁDU
        // =========================================================
        public bool CanSelectSpecification => !string.IsNullOrWhiteSpace(SelectedTask);
        public bool CanSelectMaterial => CanSelectSpecification && !string.IsNullOrWhiteSpace(SelectedSpecification);
        public bool CanSelectLocation => CanSelectMaterial && !string.IsNullOrWhiteSpace(SelectedMaterial);

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

                // 🔥 RESET KASKÁDY
                _selectedSpecification = null;
                _selectedMaterial = null;
                _selectedLocation = null;

                WorkItem = null;
                WorkUnit = null;

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSelectSpecification));
                OnPropertyChanged(nameof(CanSelectMaterial));
                OnPropertyChanged(nameof(CanSelectLocation));

                OnPropertyChanged(nameof(SelectedSpecification));
                OnPropertyChanged(nameof(SelectedMaterial));
                OnPropertyChanged(nameof(SelectedLocation));

                // 🔥 načtení UNIT
                LoadWorkUnit();

                UpdateWorkItem();
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

                _selectedMaterial = null;
                _selectedLocation = null;

                WorkItem = null;

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSelectMaterial));
                OnPropertyChanged(nameof(CanSelectLocation));

                OnPropertyChanged(nameof(SelectedMaterial));
                OnPropertyChanged(nameof(SelectedLocation));

                UpdateWorkItem();
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

                _selectedLocation = null;

                WorkItem = null;

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSelectLocation));
                OnPropertyChanged(nameof(SelectedLocation));

                UpdateWorkItem();
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

                OnPropertyChanged();
                UpdateWorkItem();
            }
        }

        // =========================================================
        // 💰 WORK ITEM
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
        // 📦 MATERIAL ITEM
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
        // 🔢 MNOŽSTVÍ
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
        // 💰 TOTAL
        // =========================================================
        public double Total
        {
            get
            {
                if (WorkItem != null)
                {
                    return WorkItem.BasePrice *
                           WorkItem.MaterialCoef *
                           WorkItem.PositionCoef *
                           Quantity;
                }

                if (MaterialItem != null)
                {
                    return MaterialItem.Price * Quantity;
                }

                return 0;
            }
        }

        // =========================================================
        // 📏 NAČTENÍ UNIT Z TASK
        // =========================================================
        private void LoadWorkUnit()
        {
            if (string.IsNullOrWhiteSpace(SelectedTask))
            {
                WorkUnit = null;
                return;
            }

            using var db = new AppDbContext();

            WorkUnit = db.PriceItems
                .AsNoTracking()
                .Where(x => x.Task == SelectedTask)
                .Select(x => x.Unit)
                .FirstOrDefault();
        }

        // =========================================================
        // 🔄 DB UPDATE WORK ITEM
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
                .AsNoTracking()
                .FirstOrDefault(x =>
                    x.Task == SelectedTask &&
                    x.Specification == SelectedSpecification &&
                    x.Material == SelectedMaterial &&
                    x.Location == SelectedLocation
                );

            OnPropertyChanged(nameof(Total));
        }

        // =========================================================
        // NOTIFIKACE UI
        // =========================================================
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}