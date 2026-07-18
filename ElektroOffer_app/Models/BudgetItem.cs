﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 📊 BudgetItem – položka detailního rozpisu (VIEW MODEL)
    // =========================================================================
    //
    // ÚČEL:
    // - Reprezentuje jeden řádek v UI rozpisu kalkulace
    // - Slouží pro zobrazení v ListView (PRÁCE / MATERIÁL)
    // - NENÍ to databázový model ani JSON model
    //
    // ROLE V ARCHITEKTUŘE:
    // - UI vrstva (WPF binding)
    // - vytvářeno v Recalculate()
    //
    // =========================================================================
    public class BudgetItem : INotifyPropertyChanged
    {
        // =========================================================
        // 🧾 TYP POLOŽKY (PRÁCE / MATERIÁL)
        // =========================================================

        private string _type = string.Empty;

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

        // =========================================================
        // 🧾 POPIS POLOŽKY
        // =========================================================

        private string _description = string.Empty;

        /// <summary>
        /// Textový popis položky.
        /// Skládá se z výběru (úkon / materiál / specifikace / umístění).
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

        // =========================================================
        // 📏 JEDNOTKA
        // =========================================================

        private string _unit = string.Empty;

        /// <summary>
        /// Měrná jednotka (m, ks, hod).
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

        // =========================================================
        // 🔢 MNOŽSTVÍ
        // =========================================================

        private double _quantity;

        /// <summary>
        /// Počet jednotek.
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

        // =========================================================
        // 💰 CENA CELKEM ZA ŘÁDEK
        // =========================================================

        private double _priceBeforeDiscount;

        /// <summary>Celková cena položky před aplikací řádkové slevy.</summary>
        public double PriceBeforeDiscount
        {
            get => _priceBeforeDiscount;
            set
            {
                if (Math.Abs(_priceBeforeDiscount - value) < 0.0001) return;
                _priceBeforeDiscount = value;
                OnPropertyChanged();
            }
        }

        private double _price;

        /// <summary>
        /// Výsledná celková cena položky po aplikaci řádkové slevy.
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

        // =========================================================
        // 🏷️ SLEVA (nově doplněno pro zobrazení v rozpisu)
        // =========================================================

        /// <summary>
        /// Procentuální sleva na tomto řádku (null = žádná sleva).
        /// Zobrazuje se v sloupci "Sleva %" v detailním rozpočtu.
        /// </summary>
        public double? DiscountPercent { get; set; }

        /// <summary>
        /// Výše slevy v Kč = cena bez slevy MINUS cena se slevou.
        /// Zobrazuje se v sloupci "Sleva Kč" v detailním rozpočtu.
        /// Null = žádná sleva na tomto řádku.
        /// </summary>
        public double? DiscountAmount { get; set; }

        // =========================================================
        // 🔔 WPF NOTIFIKACE ZMĚN
        // =========================================================

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
