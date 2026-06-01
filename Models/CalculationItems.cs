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
        // TASK
        // =========================================================
        public string? SelectedTask
        {
            get => _selectedTask;
            set
            {
                if (_selectedTask == value) return;
                _selectedTask = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
                UpdateWorkItem();
            }
        }

        // =========================================================
        // MATERIAL (WORK FILTER)
        // =========================================================
        public string? SelectedMaterial
        {
            get => _selectedMaterial;
            set
            {
                if (_selectedMaterial == value) return;
                _selectedMaterial = value;
                OnPropertyChanged();
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
        // MATERIAL ITEM (samostatná kalkulace)
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
        // MNOŽSTVÍ
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
        // 💰 TOTAL VÝPOČET
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
        // DB UPDATE LOGIKA
        // =========================================================
        private void UpdateWorkItem()
        {
            if (string.IsNullOrWhiteSpace(SelectedTask) ||
                string.IsNullOrWhiteSpace(SelectedSpecification) ||
                string.IsNullOrWhiteSpace(SelectedMaterial) ||
                string.IsNullOrWhiteSpace(SelectedLocation))
                return;

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