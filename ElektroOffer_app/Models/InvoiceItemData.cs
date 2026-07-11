using System;
using System.Collections.Generic;

namespace ElektroOffer_app.Models
{
    // ============================================================================
    // 🧾 InvoiceItemData – kompletní data faktury uložené v projektu
    // ----------------------------------------------------------------------------
    // Účel:
    //   • Obsahuje veškeré údaje potřebné pro vygenerování faktury:
    //       - údaje odběratele (zákazník)
    //       - údaje dodavatele (tvoje firma / více firem)
    //       - číslo faktury, data, poznámku
    //       - řádky faktury (Lines)
    //
    //   • Ukládá se do ProjectData → každý projekt může mít JINÉHO dodavatele.
    //     To přesně odpovídá tvému požadavku: více dodavatelů, různé údaje,
    //     možnost vyplnit jen část údajů.
    //
    //   • Povinná pole se budou validovat jen upozorněním ve ViewModelu,
    //     ale NEblokují generování PDF.
    //
    //   • Dodavatel se už NEbere z SupplierSettingsService (globální JSON).
    //     Ten může sloužit jen jako volitelná šablona, ale není povinný.
    // ============================================================================

    public class InvoiceItemData
    {
        // ------------------------------------------------------------
        // 🔢 Základní údaje faktury
        // ------------------------------------------------------------
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(14);

        // ------------------------------------------------------------
        // 🧍‍♂️ Odběratel (zákazník)
        // ------------------------------------------------------------
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string? CustomerIco { get; set; }
        public string? CustomerDic { get; set; }

        // ------------------------------------------------------------
        // 🏢 Dodavatel (tvoje firma / více firem)
        // ------------------------------------------------------------
        // Uživatel vyplňuje ručně v InvoiceWindow.
        // Může být více dodavatelů → každý projekt má vlastní údaje.
        // Povinná pole se jen upozorní, ale NEblokují generování PDF.
        public SupplierSettings Supplier { get; set; } = new SupplierSettings();

        // ------------------------------------------------------------
        // 📝 Poznámka k faktuře
        // ------------------------------------------------------------
        public string? Note { get; set; }

        // ------------------------------------------------------------
        // 📦 Řádky faktury (Práce + Materiál)
        // ------------------------------------------------------------
        public List<InvoiceLineData> Lines { get; set; } = new();
    }
}
