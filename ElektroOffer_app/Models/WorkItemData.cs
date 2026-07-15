﻿using System;

namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 🔧 WorkItemData – datový model pro jeden řádek PRÁCE v projektu
    // =========================================================================
    //
    // Účel:
    // -------
    // Tento objekt reprezentuje jednu položku v sekci PRÁCE uloženého projektu
    // (ProjectData). Obsahuje pouze vlastnosti specifické pro pracovní položky.
    //
    // Proč existuje:
    // ---------------
    // - PRÁCE a MATERIÁL mají odlišné datové struktury.
    // - CalculationItemData obsahuje pouze společné hodnoty (Quantity, sleva, Total).
    // - WorkItemData obsahuje pouze pracovní hodnoty.
    // - Díky tomu je JSON čistý, přehledný a nemíchají se nesouvisející položky.
    //
    // Proč je zde ID:
    // ---------------
    // Každý řádek PRÁCE má vlastní identifikátor (Guid Id).
    // Stejné ID se ukládá i do odpovídající položky CalculationItemData.
    //
    // Díky tomu lze jednoznačně spárovat:
    //
    //      WorkItemData.Id == CalculationItemData.Id
    //
    // To je zásadní pro stabilní Load/Save a pro budoucí rozšiřování projektu.
    //
    // 🔴 ZMĚNA – Id vs. Position:
    // ----------------------------------------------------------------
    // Id a Position mají nyní odlišný účel:
    //
    //   • Id        → jednoznačný, SEKVENČNÍ identifikátor záznamu
    //                  (W-1, W-2, W-3... podle pořadí mezi vyplněnými řádky).
    //                  Používá se výhradně pro párování se CalculationItemData.
    //                  Nemění se podle pozice řádku v UI.
    //
    //   • Position  → skutečná POZICE řádku v UI (1-based), nezávislá na Id.
    //                  Používá se výhradně pro znovuvytvoření správného
    //                  rozložení řádků při načítání projektu (ApplyProjectData).
    //
    // Dříve se pozice řádku odvozovala parsováním čísla z Id (např. "W-5" → 5),
    // což fungovalo jen do doby, než Id přestalo odpovídat pozici (např. pokud
    // je vyplněný jen 1. a 5. řádek, druhý vyplněný záznam by měl Id "W-2",
    // ale pozici 5). Oddělením obou hodnot je Id stabilní a smysluplné
    // („druhý vyplněný záznam“) a Position spolehlivě řídí rozložení v UI.
    //
    // Co se ukládá:
    // --------------
    // ✔ Id                     → jednoznačný, sekvenční identifikátor záznamu
    // ✔ Position                → skutečná pozice řádku v UI (1-based)
    // ✔ SelectedWorkTask          → název práce
    // ✔ SelectedWorkSpecification → upřesnění práce
    // ✔ SelectedBaseMaterial      → podklad
    // ✔ SelectedWorkPosition      → místo / poloha provedení
    // ✔ SelectedWorkPrice      → cena práce v době uložení projektu (volitelné)
    // ✔ SelectedWorkUnit       → měrná jednotka práce (volitelné)
    //
    // Poznámka:
    // ----------
    // Ukládají se textové hodnoty, nikoli ID z databáze.
    // Projekt je díky tomu nezávislý na konkrétních ID v ceníku.
    //
    // =========================================================================
    public class WorkItemData
    {
        // =====================================================================
        // 🆔 Id – jednoznačný, sekvenční identifikátor řádku PRÁCE
        // =====================================================================
        //
        // ID je typu string, protože používáme krátké lidsky čitelné ID:
        //   • W-1, W-2, W-3...
        //
        // Přiděluje se sekvenčně podle pořadí mezi VYPLNĚNÝMI řádky
        // (prázdné řádky se přeskakují), nezávisle na tom, na jaké pozici
        // v UI daný řádek fyzicky stojí.
        //
        // Stejné ID se ukládá i do CalculationItemData a slouží
        // výhradně k jejich spárování.
        //
        public string Id { get; set; } = string.Empty;

        // =====================================================================
        // 📍 Position – skutečná pozice řádku v UI (1-based)
        // =====================================================================
        //
        // 🔴 NOVÉ POLE
        //
        // Na rozdíl od Id se Position přiděluje podle SKUTEČNÉHO indexu
        // řádku v kolekci (WorkCalcItems) v okamžiku uložení, a to ještě
        // před odfiltrováním prázdných řádků.
        //
        // Např.: pokud uživatel vyplní jen 1. a 5. řádek, uloží se:
        //   • 1. řádek → Id = "W-1", Position = 1
        //   • 5. řádek → Id = "W-2", Position = 5
        //
        // Při načítání projektu (ApplyProjectData) se řádek vkládá na
        // index = Position - 1, takže se obnoví přesně původní rozložení
        // řádků včetně mezer mezi vyplněnými řádky.
        //
        public int Position { get; set; }

        // =====================================================================
        // 🛠 SelectedWorkTask – název práce
        // =====================================================================
        //
        // Hlavní typ úkonu, který se provádí.
        // Např.: "Montáž zásuvky", "Výměna jističe", "Tahání kabelu".
        //
        public string? SelectedWorkTask { get; set; }

        // =====================================================================
        // 📄 SelectedWorkSpecification – upřesnění práce
        // =====================================================================
        //
        // Detailní specifikace úkonu.
        // Např.: "s krabicí KP68", "bez krabice", "v liště".
        //
        public string? SelectedWorkSpecification { get; set; }

        // =====================================================================
        // 📦 SelectedBaseMaterial – podklad
        // =====================================================================
        //
        // Podklad použitý u práce.
        // Např.: "CYKY 3×2,5", "CYKY 5×6", "Lišta 40×20".
        //
        public string? SelectedBaseMaterial { get; set; }

        // =====================================================================
        // 📍 SelectedWorkPosition – místo / poloha provedení práce
        // =====================================================================
        //
        // Místo nebo poloha, kde se práce provádí.
        // Např.: "Obývák", "Kuchyň", "Chodba".
        //
        public string? SelectedWorkPosition { get; set; }

        // =====================================================================
        // 💰 SelectedWorkPrice – cena práce (volitelné)
        // =====================================================================
        //
        // Cena práce v době uložení projektu.
        // Pokud je null → cena se po načtení dopočítá z databáze.
        //
        // Používá se decimal, protože jde o peníze → přesný typ.
        //
        public decimal? SelectedWorkPrice { get; set; }

        // =====================================================================
        // 📏 SelectedWorkUnit – měrná jednotka práce (volitelné)
        // =====================================================================
        //
        // Jednotka práce (např. "m", "ks", "hod").
        // Pokud je null → jednotka se po načtení dohledá z databáze.
        //
        public string? SelectedWorkUnit { get; set; }
    }
}
