﻿namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 📦 ProjectData – kompletní uložený projekt (kalkulace)
    // =========================================================================
    //
    // Účel:
    // -------
    // Reprezentuje celý projekt uložený do JSON (Save / Load).
    // Obsahuje metadata projektu a tři oddělené datové sekce:
    //
    //   • WorkItems        → pracovní položky (WorkItemData)
    //   • MaterialItems    → materiálové položky (MaterialItemData)
    //   • CommonItems      → společné hodnoty (CalculationItemData)
    //
    // Proč existuje:
    // ---------------
    // - Umožňuje uložit celý stav kalkulace do jednoho JSON souboru.
    // - Od verze 1.7.x používá nový, čistý a stabilní datový model.
    // - PRÁCE a MATERIÁL jsou oddělené → nemíchají se nesouvisející hodnoty.
    // - Společné hodnoty jsou ukládány zvlášť → JSON je přehledný.
    //
    // =========================================================================
    public class ProjectData
    {
        // =====================================================================
        // 🏷 METADATA PROJEKTU
        // =====================================================================

        /// <summary>
        /// Název projektu / zakázky.
        /// </summary>
        public string ProjectName { get; set; } = "Nový projekt";

        /// <summary>
        /// Datum vytvoření projektu.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Datum posledního uložení projektu.
        /// Aktualizuje se při Save.
        /// </summary>
        public DateTime SavedAt { get; set; } = DateTime.Now;

        // =====================================================================
        // 🔧 PRÁCE – seznam řádků sekce PRÁCE
        // =====================================================================
        //
        // Obsahuje pouze pracovní hodnoty:
        // - SelectedWorkTask
        // - SelectedWorkSpecification
        // - SelectedBaseMaterial
        // - SelectedWorkPosition
        // - SelectedWorkPrice (volitelné)
        // - SelectedWorkUnit  (volitelné)
        //
        // Společné hodnoty (Quantity, sleva, Total) NEJSOU zde → jsou v CommonItems.
        //
        // =========================================================================
        public List<WorkItemData> WorkItems { get; set; } = new();

        // =====================================================================
        // 📦 MATERIÁL – seznam řádků sekce MATERIÁL
        // =====================================================================
        //
        // Obsahuje pouze materiálové hodnoty:
        // - SelectedCategory
        // - SelectedProductName
        // - SelectedSupplier
        // - SelectedOffer
        // - SelectedMaterialPrice (volitelné)
        // - SelectedMaterialUnit  (volitelné)
        //
        // Společné hodnoty (Quantity, sleva, Total) NEJSOU zde → jsou v CommonItems.
        //
        // =========================================================================
        public List<MaterialItemData> MaterialItems { get; set; } = new();

        // =====================================================================
        // 🧮 SPOLEČNÉ – společné hodnoty PRÁCE i MATERIÁLU
        // =====================================================================
        //
        // Obsahuje pouze hodnoty, které jsou společné pro oba typy položek:
        // - Quantity
        // - DiscountPercent
        // - IsDiscountEnabled
        // - Total
        //
        // Každý řádek v CommonItems odpovídá jednomu řádku v WorkItems nebo MaterialItems.
        //
        // =========================================================================
        public List<CalculationItemData> CommonItems { get; set; } = new();

        /// <summary>
        /// Volitelný uložený návrh faktury navázaný na projekt.
        /// </summary>
        public ElektroOffer_app.Invoice.Models.InvoiceDraft? InvoiceDraft { get; set; }

        /// <summary>
        /// Počet viditelných řádků v sekci PRÁCE, včetně prázdných řádků.
        /// </summary>
        public int WorkRowCount { get; set; } = 5;

        /// <summary>
        /// Počet viditelných řádků v sekci MATERIÁL, včetně prázdných řádků.
        /// </summary>
        public int MaterialRowCount { get; set; } = 5;
    }
}
