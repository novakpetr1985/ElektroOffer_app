using System.ComponentModel;
using System.Runtime.CompilerServices;
using ElektroOffer_app.Models;

namespace ElektroOffer_app
{
    // =========================
    // 🧮 JEDEN ŘÁDEK KALKULACE
    // =========================
    public class CalculationItems : INotifyPropertyChanged
    {
        private PriceItems? _item;
        private double _quantity;

        // =========================
        // vybraná položka (práce / materiál)
        // =========================
        public PriceItems? Item
        {
            get => _item;
            set
            {
                _item = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        // =========================
        // množství
        // =========================
        public double Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        // =========================
        // výpočet řádku
        // =========================
        public double Total
        {
            get
            {
                if (Item == null) return 0;

                return Item.BasePrice *
                       Item.MaterialCoef *
                       Item.PositionCoef *
                       Quantity;
            }
        }

        // =========================
        // PROPERTY CHANGED
        // =========================
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}