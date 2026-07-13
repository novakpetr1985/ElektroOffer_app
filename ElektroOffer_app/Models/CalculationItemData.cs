using System;

namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 🧮 CalculationItemData – společný DTO pro PRÁCI i MATERIÁL
    // =========================================================================
    //
    // Tento objekt slouží jako *univerzální transportní model* pro ukládání
    // a načítání kompletního obsahu řádků kalkulace:
    //
    //   • PRÁCE     → WorkTask, WorkSpecification, BaseMaterial, Position
    //   • MATERIÁL  → Category, ProductName, Supplier, Offer, MaterialPrice, MaterialUnit
    //
    // CalculationItemData obsahuje:
    //   ✔ společné hodnoty (Quantity, sleva, Total)
    //   ✔ kompletní pracovní obsah (WorkTaskName…)
    //   ✔ kompletní materiálový obsah (CategoryName…)
    //
    // Proč je to správně:
    // -------------------
    // - WorkItemData / MaterialItemData obsahují pouze metadata (Id + Position)
    // - CalculationItemData obsahuje kompletní obsah položky
    // - JSON je čistý, přehledný a jednoznačný
    // - Load/Save je stabilní a typově bezpečný
    //
    // =========================================================================
    public class CalculationItemData
    {
        // =====================================================================
        // 🆔 Id – jednoznačný identifikátor řádku (W-1 / M-1)
        // =====================================================================
        public string Id { get; set; } = string.Empty;

        // =====================================================================
        // 📏 Quantity – množství položky
        // =====================================================================
        public double Quantity { get; set; }

        // =====================================================================
        // 💸 DiscountPercent – procentuální sleva
        // =====================================================================
        public double? DiscountPercent { get; set; }

        // =====================================================================
        // 🔘 IsDiscountEnabled – příznak aktivní slevy
        // =====================================================================
        public bool IsDiscountEnabled { get; set; }

        // =====================================================================
        // 🧮 Total – výsledná cena po slevě
        // =====================================================================
        public double Total { get; set; }

        // =====================================================================
        // 🔵 PRÁCE – typově bezpečná kaskáda práce
        // =====================================================================
        //
        // Tyto hodnoty se ukládají do JSONu, aby bylo možné přesně obnovit
        // celý pracovní řádek při načítání projektu.
        //
        public string? WorkTaskName { get; set; }
        public string? WorkSpecificationName { get; set; }
        public string? BaseMaterialName { get; set; }
        public string? PositionName { get; set; }

        // Jednotka práce (např. ks, m, hod)
        public string? WorkUnit { get; set; }

        // Cena práce (z WorkTask.BasePrice)
        public decimal? WorkPrice { get; set; }

        // =====================================================================
        // 🔵 MATERIÁL – typově bezpečná kaskáda materiálu
        // =====================================================================
        //
        // Tyto hodnoty se ukládají do JSONu, aby bylo možné přesně obnovit
        // celý materiálový řádek při načítání projektu.
        //
        public string? CategoryName { get; set; }
        public string? ProductName { get; set; }
        public string? SupplierName { get; set; }
        public string? OfferName { get; set; }

        // Cena materiálu (z dodavatelské nabídky)
        public decimal? MaterialPrice { get; set; }

        // Jednotka materiálu (např. ks, m, balení)
        // Ukládá se z SelectedMaterialPrice.Unit
        public string? MaterialUnit { get; set; }
    }
}
