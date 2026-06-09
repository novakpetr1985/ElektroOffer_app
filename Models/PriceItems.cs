﻿namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 🔧 PriceItems – jedna položka ceníku práce
    // =========================================================================
    //
    // K čemu slouží:
    // - Reprezentuje jeden řádek ceníku práce v databázi (tabulka PriceItems)
    // - Obsahuje základní cenu a koeficienty pro výpočet výsledné ceny
    //
    // Vlastnosti:
    // - Id            → primární klíč
    // - BasePrice     → základní cena (bez koeficientů)
    // - Unit          → měrná jednotka (m, ks, hod, …)
    // - Task          → název práce (např. "Drážkování")
    // - Specification → upřesnění (např. "Spára")
    // - Material      → typ materiálu (např. "Omítka - Sádra")
    // - Location      → umístění (např. "Stěna")
    // - MaterialCoef  → koeficient materiálu
    // - PositionCoef  → koeficient polohy/umístění
    //
    // FullName:
    // - Složený text pro zobrazení v ComboBoxu (Task | Specification | Material | Location)
    // =========================================================================
    public class PriceItems
    {
        public int Id { get; set; }

        public double BasePrice { get; set; }

        public string? Unit { get; set; }

        public string Task { get; set; } = string.Empty;

        public string Specification { get; set; } = string.Empty;

        public string Material { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public double MaterialCoef { get; set; }

        public double PositionCoef { get; set; }

        /// <summary>
        /// Složený text pro zobrazení v ComboBoxu.
        /// Umožňuje uživateli snadno rozlišit položky podle všech parametrů.
        /// </summary>
        public string FullName =>
            $"{Task} | {Specification} | {Material} | {Location}";
    }
}
