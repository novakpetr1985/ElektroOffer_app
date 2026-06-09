﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 📊 BudgetItem – jedna položka v detailním rozpisu
    // =========================================================================
    //
    // K čemu slouží:
    // - Reprezentuje jeden řádek v přehledovém rozpisu (PRÁCE / MATERIÁL)
    // - Používá se v kolekci BudgetItems v MainWindow
    //
    // Vlastnosti:
    // - Type        → typ položky ("PRÁCE" nebo "MATERIÁL")
    // - Description → textový popis (sestavený z vybraných hodnot)
    // - Unit        → měrná jednotka (m, ks, hod, …)
    // - Quantity    → množství
    // - Price       → výsledná cena za řádek
    //
    // Implementuje INotifyPropertyChanged:
    // - Umožňuje WPF automaticky aktualizovat UI při změně hodnot
    // =========================================================================
    public class BudgetItem : INotifyPropertyChanged
    {
        private string _type = "";

        /// <summary>
        /// Typ položky v rozpisu.
        /// Např. "PRÁCE" nebo "MATERIÁL".
        /// </summary>
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

        private string _description = "";

        /// <summary>
        /// Textový popis položky (např. kombinace úkonu, specifikace, materiálu, umístění).
        /// </summary>
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

        private string _unit = "";

        /// <summary>
        /// Měrná jednotka (např. "m", "ks", "hod").
        /// </summary>
        public string Unit
        {
            get => _unit;
            set
            {
                if (_unit == value) return;
                _unit = value;
                OnPropertyChanged();
            }
        }

        private double _quantity;

        /// <summary>
        /// Množství (počet jednotek).
        /// </summary>
        public double Quantity
        {
            get => _quantity;
            set
            {
                if (Math.Abs(_quantity - value) < 0.0001) return;
                _quantity = value;
                OnPropertyChanged();
            }
        }

        private double _price;

        /// <summary>
        /// Celková cena za tuto položku (Quantity × jednotková cena).
        /// </summary>
        public double Price
        {
            get => _price;
            set
            {
                if (Math.Abs(_price - value) < 0.0001) return;
                _price = value;
                OnPropertyChanged();
            }
        }

        // ---------------------------------------------------------------------
        // INotifyPropertyChanged – notifikace změn pro WPF binding
        // ---------------------------------------------------------------------
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
