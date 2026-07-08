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
    // Co se ukládá:
    // --------------
    // ✔ Id                     → jednoznačný identifikátor řádku
    // ✔ SelectedTask           → název práce
    // ✔ SelectedSpecification  → upřesnění práce
    // ✔ SelectedMaterial       → použitý materiál
    // ✔ SelectedLocation       → místo provedení
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
        // 🆔 Id – jednoznačný identifikátor řádku PRÁCE
        // =====================================================================
        //
        // ID je typu string, protože používáme krátké lidsky čitelné ID:
        //   • W-1, W-2, W-3...
        //
        // Stejné ID se ukládá i do CalculationItemData.
        //
        public string Id { get; set; } = string.Empty;

        // =====================================================================
        // 🛠 SelectedTask – název práce
        // =====================================================================
        //
        // Hlavní typ úkonu, který se provádí.
        // Např.: "Montáž zásuvky", "Výměna jističe", "Tahání kabelu".
        //
        public string? SelectedTask { get; set; }

        // =====================================================================
        // 📄 SelectedSpecification – upřesnění práce
        // =====================================================================
        //
        // Detailní specifikace úkonu.
        // Např.: "s krabicí KP68", "bez krabice", "v liště".
        //
        public string? SelectedSpecification { get; set; }

        // =====================================================================
        // 📦 SelectedMaterial – použitý materiál
        // =====================================================================
        //
        // Materiál použitý u práce.
        // Např.: "CYKY 3×2,5", "CYKY 5×6", "Lišta 40×20".
        //
        public string? SelectedMaterial { get; set; }

        // =====================================================================
        // 📍 SelectedLocation – místo provedení práce
        // =====================================================================
        //
        // Lokace, kde se práce provádí.
        // Např.: "Obývák", "Kuchyň", "Chodba".
        //
        public string? SelectedLocation { get; set; }

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
