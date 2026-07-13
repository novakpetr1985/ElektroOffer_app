using System;

namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 🔧 WorkItemData – datový model pro jeden řádek PRÁCE v projektu
    // =========================================================================
    //
    // Účel:
    // -------
    // Tento objekt reprezentuje jednu pracovní položku uloženého projektu
    // (ProjectData). Obsahuje pouze hodnoty nutné pro znovuvytvoření struktury
    // řádků v UI – nikoliv samotná pracovní data.
    //
    // Proč existuje:
    // ---------------
    // - CalculationItemData ukládá společné hodnoty (Quantity, sleva, Total).
    // - WorkItemData ukládá pouze metadata řádku PRÁCE:
    //       • Id        – stabilní identifikátor záznamu
    //       • Position  – skutečná pozice řádku v UI
    //
    // - Pracovní obsah (WorkTask, WorkSpecification, BaseMaterial, Position,
    //   WorkPrice, WorkUnit) se ukládá do CalculationItemData.
    //
    // - Díky tomu je JSON přehledný, nemíchají se pracovní a společné hodnoty
    //   a načítání projektu je stabilní.
    //
    // Proč je zde ID:
    // ---------------
    // Každý řádek PRÁCE má vlastní identifikátor (např. "W-1", "W-2", ...).
    // Stejné ID se ukládá i do CalculationItemData.
    //
    // Díky tomu lze jednoznačně spárovat:
    //
    //      WorkItemData.Id == CalculationItemData.Id
    //
    // To je zásadní pro stabilní Load/Save a pro budoucí rozšiřování projektu.
    //
    // 🔴 Id vs. Position – rozdílné role
    // ----------------------------------------------------------------
    // • Id
    //     – sekvenční identifikátor mezi VYPLNĚNÝMI řádky
    //     – nemění se podle pozice v UI
    //     – slouží výhradně pro párování s CalculationItemData
    //
    // • Position
    //     – skutečná pozice řádku v UI (1-based)
    //     – používá se při načítání projektu (ApplyProjectData)
    //     – umožňuje obnovit přesné rozložení řádků včetně mezer
    //
    // Co se ukládá:
    // --------------
    // ✔ Id        → stabilní identifikátor řádku
    // ✔ Position  → pozice řádku v UI
    //
    // Poznámka:
    // ----------
    // Pracovní obsah (WorkTask, WorkSpecification, BaseMaterial, Position,
    // WorkPrice, WorkUnit) se ukládá do CalculationItemData.
    // WorkItemData obsahuje pouze metadata řádku.
    //
    // =========================================================================
    public class WorkItemData
    {
        // =====================================================================
        // 🆔 Id – stabilní identifikátor řádku PRÁCE
        // =====================================================================
        public string Id { get; set; } = string.Empty;

        // =====================================================================
        // 📍 Position – skutečná pozice řádku v UI (1-based)
        // =====================================================================
        public int Position { get; set; }
    }
}
