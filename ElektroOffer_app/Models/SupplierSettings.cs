using System;

namespace ElektroOffer_app.Models
{
    // ============================================================================
    // 🏢 SupplierSettings – trvalé údaje o dodavateli (tobě jako firmě/OSVČ)
    // ----------------------------------------------------------------------------
    // Účel:
    //   • Údaje se nastavují JEDNOU v Options/Settings okně, ne opakovaně pro
    //     každou fakturu – jsou stejné napříč všemi projekty.
    //   • Ukládají se do samostatného JSON configu appky (mimo ProjectData),
    //     protože se netýkají konkrétního projektu, ale instalace appky jako
    //     celku. Persistence bude řešena v rámci SettingsWindow feature.
    //
    // Použití:
    //   • InvoiceDocument (QuestPDF layout) čte tato data pro hlavičku faktury
    //     (údaje dodavatele) a pro rozhodnutí, zda zobrazit sloupec DPH.
    //
    // 🔴 IsVatPayer:
    //   • Řídí, jestli InvoiceDocument vykreslí tabulku položek se sloupci
    //     Základ/DPH/Celkem, nebo jednodušší tabulku jen s Celkem.
    //   • Pokud false, VatRate u jednotlivých InvoiceLineData se ignoruje.
    // ============================================================================
    public class SupplierSettings
    {
        public string CompanyName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Ico { get; set; } = string.Empty;

        // Nepovinné – relevantní jen když IsVatPayer == true
        public string? Dic { get; set; }

        public string BankAccount { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // 🔴 Řídí zobrazení DPH na faktuře (viz komentář výše)
        public bool IsVatPayer { get; set; } = false;
    }
}