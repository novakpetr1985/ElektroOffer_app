namespace ElektroOffer_app.Models
{
    // ============================================================================
    // 📄 InvoiceLineData – jeden řádek faktury (položka)
    // ----------------------------------------------------------------------------
    // Účel:
    //   • Reprezentuje jeden řádek v tabulce faktury – ať už vznikl z pracovní
    //     položky (WorkCalcItems) nebo materiálové položky (MaterialItems).
    //   • Je to zjednodušený "výstupní" model – neobsahuje kaskádová pole
    //     (Task/Specification/... nebo Category/ProductName/...), jen to, co
    //     se skutečně zobrazí na faktuře.
    //
    // Mapování (řešeno v InvoiceTemplateService):
    //   • Description = poskládaný text z kaskády (např. "Task – Specification,
    //     Material, Location" u Práce, nebo "ProductName (Supplier)" u Materiálu)
    //   • Quantity, Unit, UnitPrice = přímo z CalculationItemViewModel
    //
    // 🔴 VatRate:
    //   • Nepovinné – použije se jen když SupplierSettings.IsVatPayer == true.
    //   • Výchozí hodnota 21 % (základní sazba ČR), lze upravit per položku
    //     v budoucí verzi, pokud bude potřeba snížená sazba.
    //
    // 🔴 LineType:
    //   • Textový typ řádku (např. "Práce", "Materiál").
    //   • Umožňuje v UI / PDF rozlišit, z jaké části kalkulace řádek pochází.
    //
    // 🔴 DiscountPercent:
    //   • Procentuální sleva přenesená z kalkulace (CalculationItemViewModel).
    //   • Pokud sleva není zapnutá, je null – řádek se chová jako bez slevy.
    // ============================================================================
    public class InvoiceLineData
    {
        // Popis řádku – uživatelsky čitelný text (bez vnitřní kaskády).
        public string Description { get; set; } = string.Empty;

        // Množství (např. hodiny, kusy).
        public double Quantity { get; set; }

        // Jednotka (např. "hod", "ks").
        public string Unit { get; set; } = string.Empty;

        // Jednotková cena bez DPH.
        public decimal UnitPrice { get; set; }

        // Nepovinné, relevantní jen pro plátce DPH.
        // Pokud je dodavatel neplátce, může být ignorováno.
        public double? VatRate { get; set; } = 21;

        // Typ řádku – např. "Práce" nebo "Materiál".
        // Umožňuje v PDF nebo UI vizuálně odlišit různé druhy položek.
        public string? LineType { get; set; }

        // Procentuální sleva (0–100), pokud byla v kalkulaci zapnutá.
        // Pokud sleva není použita, je null.
        public double? DiscountPercent { get; set; }

        // Pomocná vlastnost pro výpočet součtu řádku (bez DPH, bez slevy).
        // Logika slevy může být aplikována až ve výpočtech faktury.
        public decimal LineTotal => (decimal)Quantity * UnitPrice;
    }
}
