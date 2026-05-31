using System.ComponentModel;
using System.Runtime.CompilerServices;
using ElektroOffer_app.Models;

namespace ElektroOffer_app
{
    // =========================================================
    // 🧮 KALKULAČNÍ ŘÁDEK
    // =========================================================
    // 👉 Jeden řádek kalkulace (PRÁCE nebo MATERIÁL)
    // 👉 Obsahuje výběr položky + množství + výpočet ceny
    // 👉 NEUKLÁDÁ se do DB (jen UI model)
    // =========================================================
    public class CalculationItems : INotifyPropertyChanged
    {
        // =========================
        // 🔧 PRÁCE (ceníková položka)
        // =========================
        private PriceItems? _workItem;

        // =========================
        // 📦 MATERIÁL (položka materiálu)
        // =========================
        private Material? _materialItem;

        // =========================
        // 📊 MNOŽSTVÍ (ks, m, hodiny…)
        // =========================
        private double _quantity;

        // =========================================================
        // 🔧 VÝBĚR PRÁCE
        // =========================================================
        public PriceItems? WorkItem
        {
            get => _workItem;
            set
            {
                _workItem = value;

                // 👉 přepočet UI
                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        // =========================================================
        // 📦 VÝBĚR MATERIÁLU
        // =========================================================
        public Material? MaterialItem
        {
            get => _materialItem;
            set
            {
                _materialItem = value;

                // 👉 přepočet UI
                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        // =========================================================
        // 📊 MNOŽSTVÍ
        // =========================================================
        public double Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;

                // 👉 vždy přepočítat řádek
                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        // =========================================================
        // 💰 VÝPOČET ŘÁDKU
        // =========================================================
        public double Total
        {
            get
            {
                // =========================
                // 🧠 PRÁCE
                // =========================
                if (WorkItem != null)
                {
                    return WorkItem.BasePrice
                         * WorkItem.MaterialCoef
                         * WorkItem.PositionCoef
                         * Quantity;
                }

                // =========================
                // 🧠 MATERIÁL
                // =========================
                if (MaterialItem != null)
                {
                    return MaterialItem.Price * Quantity;
                }

                return 0;
            }
        }

        // =========================================================
        // 🔔 UI NOTIFIKACE
        // =========================================================
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}