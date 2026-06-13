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
    //
    // Poznámka:
    // - Ukládají se textové hodnoty, ne ID → projekt je nezávislý na konkrétních ID v DB.
    // =========================================================================
    public class WorkItemData
    {
        public string? SelectedTask { get; set; }
        public string? SelectedSpecification { get; set; }
        public string? SelectedMaterial { get; set; }
        public string? SelectedLocation { get; set; }
        public double Quantity { get; set; }
    }
}
