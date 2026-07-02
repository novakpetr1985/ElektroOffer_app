﻿namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 🔧 WorkItemData – data pro jeden řádek práce v projektu
    // =========================================================================
    //
    // K čemu slouží:
    // - Reprezentuje jeden řádek sekce PRÁCE v uloženém projektu (ProjectData)
    // - Ukládá se do JSON při Save a načítá při Load
    //
    // Vlastnosti:
    // - SelectedTask          → vybraný úkon (název práce)
    // - SelectedSpecification → upřesnění
    // - SelectedMaterial      → typ materiálu
    // - SelectedLocation      → umístění
    // - Quantity              → množství
    // - IsDiscountEnabled     → příznak aktivace slevy na tomto řádku
    // - DiscountPercent       → procentuální výše slevy (null = nezadána)
    //
    // Poznámka:
    // - Ukládají se textové hodnoty, ne ID → projekt je nezávislý na konkrétních ID v DB.
    // - IsDiscountEnabled výchozí false → staré .eof soubory bez slevy se načtou správně.
    // =========================================================================
    public class WorkItemData
    {
        public string? SelectedTask { get; set; }
        public string? SelectedSpecification { get; set; }
        public string? SelectedMaterial { get; set; }
        public string? SelectedLocation { get; set; }
        public double Quantity { get; set; }

        // ---------------------- SLEVA ----------------------

        /// <summary>
        /// Příznak aktivace slevy na tomto řádku.
        /// Výchozí false → staré .eof soubory bez slevy se načtou správně.
        /// </summary>
        public bool IsDiscountEnabled { get; set; }

        /// <summary>
        /// Procentuální výše slevy (0–100).
        /// Null = sleva není zadána.
        /// </summary>
        public double? DiscountPercent { get; set; }
    }
}