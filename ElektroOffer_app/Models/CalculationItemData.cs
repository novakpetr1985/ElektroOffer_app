using System;

namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 🧮 CalculationItemData – společný DTO pro PRÁCI i MATERIÁL
    // =========================================================================
    //
    // Účel:
    // -------
    // Tento objekt slouží jako *univerzální transportní model* pro ukládání
    // a načítání společných hodnot obou typů položek:
    //
    //   • PRÁCE     → práce, specifikace, materiál, lokace (řeší ViewModel)
    //   • MATERIÁL  → kategorie, název, dodavatel, nabídka (řeší ViewModel)
    //
    // CalculationItemData obsahuje pouze ty vlastnosti, které jsou
    // společné pro oba typy položek a které se ukládají do JSONu.
    //
    // Proč je to správně:
    // -------------------
    // - PRÁCE a MATERIÁL mají odlišné datové struktury → nesmí se míchat.
    // - Společné hodnoty (množství, sleva, total) jsou opravdu společné.
    // - ViewModel řeší logiku PRÁCE i MATERIÁLU, ale JSON má být čistý.
    // - CalculationItemData je díky tomu jednoduchý, stabilní a přehledný.
    //
    // Proč je zde ID:
    // ----------------
    // - Každý řádek PRÁCE nebo MATERIÁLU má svůj vlastní ID.
    // - Společná položka (CalculationItemData) má stejné ID jako odpovídající
    //   WorkItemData nebo MaterialItemData.
    //
    //   • PRÁCE     → W-1, W-2, W-3...
    //   • MATERIÁL  → M-1, M-2, M-3...
    //
    // - Díky tomu lze jednoznačně spárovat:
    //
    //       WorkItems[i].Id == CommonItems[j].Id
    //       MaterialItems[i].Id == CommonItems[j].Id
    //
    // - JSON je díky tomu jednoznačný, stabilní a bezpečný pro Load/Save.
    //
    // Co se ukládá:
    // --------------
    // ✔ Id                → jednoznačný identifikátor řádku (W-1 / M-1)
    // ✔ Quantity          → množství položky
    // ✔ DiscountPercent   → procentuální sleva
    // ✔ IsDiscountEnabled → zda je sleva aktivní
    // ✔ Total             → výsledná cena po slevě
    //
    // Co se NEukládá:
    // ----------------
    // ✘ SelectedWorkTask, SelectedWorkSpecification, SelectedBaseMaterial, SelectedWorkPosition
    // ✘ SelectedCategory, SelectedProductName, SelectedSupplier, SelectedOffer
    // ✘ SelectedMaterialPrice, SelectedMaterialUnit
    //
    // Tyto hodnoty se ukládají v WorkItemData / MaterialItemData,
    // protože patří pouze jednomu typu položky.
    //
    // =========================================================================
    public class CalculationItemData
    {
        // =====================================================================
        // 🆔 Id – jednoznačný identifikátor řádku
        // =====================================================================
        //
        // ID je typu string, protože používáme krátké lidsky čitelné ID:
        //   • W-1, W-2, W-3...  (položky PRÁCE)
        //   • M-1, M-2, M-3...  (položky MATERIÁLU)
        //
        // CalculationItemData.Id je vždy stejné jako WorkItemData.Id
        // nebo MaterialItemData.Id.
        //
        // Díky tomu lze při načítání projektu přesně určit,
        // která společná položka patří ke které pracovní nebo materiálové položce.
        //
        public string Id { get; set; } = string.Empty;

        // =====================================================================
        // 📏 Quantity – množství položky
        // =====================================================================
        //
        // Společná hodnota pro PRÁCI i MATERIÁL.
        // Udává počet kusů, metrů, hodin… podle typu položky.
        //
        public double Quantity { get; set; }

        // =====================================================================
        // 💸 DiscountPercent – procentuální sleva
        // =====================================================================
        //
        // Hodnota slevy v procentech (např. 10 = sleva 10 %).
        // Pokud je null → sleva není nastavena.
        //
        public double? DiscountPercent { get; set; }

        // =====================================================================
        // 🔘 IsDiscountEnabled – příznak aktivní slevy
        // =====================================================================
        //
        // Určuje, zda se má sleva aplikovat.
        // false = sleva se ignoruje, i když DiscountPercent má hodnotu.
        //
        public bool IsDiscountEnabled { get; set; }

        // =====================================================================
        // 🧮 Total – výsledná cena po slevě
        // =====================================================================
        //
        // Finální vypočtená cena položky.
        // Výpočet probíhá ve ViewModelu / CalculationPriceService.
        //
        public double Total { get; set; }
    }
}
