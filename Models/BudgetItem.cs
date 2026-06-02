using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElektroOffer_app.Models
{
    // =========================================================
    // 📊 POLOŽKA DO DETAILNÍHO ROZPISU
    // =========================================================
    public class BudgetItem : INotifyPropertyChanged
    {
        // ---------------------------------------------------------
        // 🧩 TYPE - typ položky (např. materiál, práce)
        // ---------------------------------------------------------
        private string _type;
        public string Type
        {
            get => _type;
            set
            {
                if (_type == value) return;

                _type = value;
                OnPropertyChanged();
            }
        }

        // ---------------------------------------------------------
        // 🧩 DESCRIPTION - popis položky
        // ---------------------------------------------------------
        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                if (_description == value) return;

                _description = value;
                OnPropertyChanged();
            }
        }

        // ---------------------------------------------------------
        // 💰 PRICE - cena položky
        // ---------------------------------------------------------
        private double _price;
        public double Price
        {
            get => _price;
            set
            {
                if (_price == value) return;

                _price = value;
                OnPropertyChanged();
            }
        }

        // ---------------------------------------------------------
        // 🔔 INotifyPropertyChanged IMPLEMENTACE
        // ---------------------------------------------------------

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}