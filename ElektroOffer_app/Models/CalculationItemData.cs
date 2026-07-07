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
    //   • PRÁCE   → práce, specifikace, materiál, lokace (řeší ViewModel)
    //   • MATERIÁL → kategorie, název, dodavatel, nabídka (řeší ViewModel)
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
    // Co se ukládá:
    // --------------
    // ✔ Quantity           → množství položky
    // ✔ DiscountPercent    → procentuální sleva
    // ✔ IsDiscountEnabled  → zda je sleva aktivní
    // ✔ Total              → výsledná cena po slevě
    //
    // Co se NEukládá:
    // ----------------
    // ✘ SelectedTask, SelectedSpecification, SelectedMaterial, SelectedLocation
    // ✘ SelectedCategory, SelectedProductName, SelectedSupplier, SelectedOffer
    // ✘ SelectedMaterialPrice, SelectedMaterialUnit
    //
    // Tyto hodnoty se ukládají přímo v ProjectService (BuildProjectData),
    // protože patří pouze jednomu typu položky (buď PRÁCE, nebo MATERIÁL).
    //
    // =========================================================================
    public class CalculationItemData
    {
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
