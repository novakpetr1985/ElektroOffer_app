﻿namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 📦 ProjectData – kompletní uložený projekt (kalkulace)
    // =========================================================================
    //
    // Účel:
    // -------
    // Reprezentuje celý projekt uložený do JSON (Save / Load).
    // Obsahuje metadata projektu a čtyři oddělené datové sekce:
    //
    //   • WorkItems        → pracovní položky (WorkItemData)
    //   • MaterialItems    → materiálové položky (MaterialItemData)
    //   • CommonItems      → společné hodnoty (CalculationItemData)
    //   • InvoiceData      → fakturační údaje projektu (InvoiceItemData), volitelné
    //
    // Proč existuje:
    // ---------------
    // - Umožňuje uložit celý stav kalkulace do jednoho JSON souboru.
    // - Od verze 1.7.x používá nový, čistý a stabilní datový model.
    // - PRÁCE a MATERIÁL jsou oddělené → nemíchají se nesouvisející hodnoty.
    // - Společné hodnoty jsou ukládány zvlášť → JSON je přehledný.
    // - Od verze 1.8.1 lze k projektu volitelně připojit fakturační údaje.
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
        // - SelectedTask
        // - SelectedSpecification
        // - SelectedMaterial
        // - SelectedLocation
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

        // =====================================================================
        // 🧾 FAKTURACE – fakturační údaje projektu (volitelné)
        // =====================================================================
        //
        // Obsahuje údaje potřebné k vygenerování faktury (InvoiceDocument):
        // - Číslo faktury, datum vystavení/splatnosti
        // - Údaje odběratele (jméno, adresa, IČO, DIČ)
        // - Řádky faktury (InvoiceLineData) – snímek položek kalkulace
        //   v okamžiku generování faktury
        //
        // Je nullable záměrně:
        // - Projekt může existovat bez toho, aby z něj byla někdy vygenerována
        //   faktura (např. pouze nabídka pro zákazníka).
        // - Starší uložené projekty (před zavedením fakturace v 1.8.1) se bez
        //   problému deserializují s InvoiceData == null, žádná migrace
        //   souborů není potřeba.
        //
        // Údaje DODAVATELE (firma, IČO, DIČ, plátce DPH) zde NEJSOU – ty jsou
        // trvalé napříč všemi projekty a ukládají se zvlášť do SupplierSettings
        // (app-level config, viz Options/Settings okno), ne do jednotlivých
        // projektových JSON souborů.
        //
        // =========================================================================

        /// <summary>
        /// Fakturační údaje projektu. Null, pokud faktura ještě nebyla vygenerována.
        /// </summary>
        public InvoiceItemData? InvoiceData { get; set; }
    }
}