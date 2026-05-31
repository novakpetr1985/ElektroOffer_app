using System.ComponentModel;
using System.Runtime.CompilerServices;
using ElektroOffer_app.Models;

namespace ElektroOffer_app
{
    // =========================
    // 🧮 KALKULAČNÍ ŘÁDEK
    // =========================
    // 👉 Jeden řádek kalkulace (práce nebo materiál)
    // 👉 Automaticky přepočítává cenu při změnách
    // =========================
    public class CalculationItems : INotifyPropertyChanged
    {
        // =========================
        // 🔧 PRÁCE (CENÍKOVÁ POLOŽKA)
        // =========================
        private PriceItems? _workItem;

        // =========================
        // 📦 MATERIÁL
        // =========================
        private Material? _materialItem;

        // =========================
        // 📊 MNOŽSTVÍ
        // =========================
        private double _quantity;

        // =========================
        // 🔧 VÝBĚR PRÁCE
        // =========================
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

        // =========================
        // 📦 VÝBĚR MATERIÁLU
        // =========================
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

        // =========================
        // 📊 MNOŽSTVÍ
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
        // 💰 VÝPOČET ŘÁDKU
        // =========================
        public double Total
        {
            get
            {
                // 👉 výpočet práce
                if (WorkItem != null)
                {
                    return WorkItem.BasePrice *
                           WorkItem.MaterialCoef *
                           WorkItem.PositionCoef *
                           Quantity;
                }

                // 👉 výpočet materiálu
                if (MaterialItem != null)
                {
                    return MaterialItem.Price * Quantity;
                }

                return 0;
            }
        }

        // =========================
        // 🔔 NOTIFIKACE UI
        // =========================
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}