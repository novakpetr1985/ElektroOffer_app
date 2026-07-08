﻿using System;

namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 📦 MaterialItemData – datový model pro jeden řádek MATERIÁLU v projektu
    // =========================================================================
    //
    // Účel:
    // -------
    // Tento objekt reprezentuje jednu položku v sekci MATERIÁL uloženého
    // projektu (ProjectData). Obsahuje pouze ty vlastnosti, které jsou
    // specifické pro materiál – tedy informace o produktu, dodavateli,
    // nabídce a ceně.
    //
    // Proč existuje:
    // ---------------
    // - PRÁCE a MATERIÁL mají odlišné datové struktury.
    // - CalculationItemData obsahuje pouze společné hodnoty (Quantity, sleva, Total).
    // - MaterialItemData obsahuje pouze materiálové hodnoty.
    // - Díky tomu je JSON čistý, přehledný a nemíchají se nesouvisející položky.
    //
    // Proč je zde ID:
    // ---------------
    // Každý řádek MATERIÁLU má vlastní identifikátor (Guid Id).
    // Stejné ID se ukládá i do odpovídající položky CalculationItemData.
    //
    // Díky tomu lze jednoznačně spárovat:
    //
    //      MaterialItemData.Id == CalculationItemData.Id
    //
    // To je zásadní pro stabilní Load/Save a pro budoucí rozšiřování projektu.
    //
    // Co se ukládá:
    // --------------
    // ✔ Id                     → jednoznačný identifikátor řádku
    // ✔ SelectedCategory        → hlavní kategorie materiálu (např. Chrániče)
    // ✔ SelectedProductName     → název produktu (např. FH202 AC-40/0,03)
    // ✔ SelectedSupplier        → dodavatel (např. ELKOV)
    // ✔ SelectedOffer           → konkrétní nabídka dodavatele (string)
    // ✔ SelectedMaterialPrice   → cena materiálu v době uložení projektu
    // ✔ SelectedMaterialUnit    → jednotka (ks, m, bm…)
    //
    // Poznámka k ceně:
    // ----------------
    // Cena je volitelná. Pokud ji uložíš:
    //   • projekt bude používat historickou cenu (správné pro nabídky)
    //
    // Pokud ji NEuložíš:
    //   • cena se po načtení dopočítá z databáze (správné pro dynamické ceny)
    //
    // =========================================================================
    public class MaterialItemData
    {
        // =====================================================================
        // 🆔 Id – jednoznačný identifikátor řádku MATERIÁLU
        // =====================================================================
        //
        // ID je typu string, protože používáme krátké lidsky čitelné ID:
        //   • M-1, M-2, M-3...
        //
        // Stejné ID se ukládá i do CalculationItemData.
        //
        public string Id { get; set; } = string.Empty;

        // =====================================================================
        // 🏷 Kategorie materiálu
        // =====================================================================
        //
        // Hlavní skupina produktu (např. "Chrániče").
        // Slouží k obnově stromu kategorií při načítání projektu.
        //
        public string? SelectedCategory { get; set; }

        // =====================================================================
        // 📄 Název produktu
        // =====================================================================
        //
        // Konkrétní produkt v rámci kategorie (např. "Chránič proudový 2-pólový").
        // Používá se k dohledání dostupných dodavatelů.
        //
        public string? SelectedProductName { get; set; }

        // =====================================================================
        // 🏢 Dodavatel
        // =====================================================================
        //
        // Název dodavatele (např. "ELKOV").
        // Slouží k dohledání dostupných nabídek.
        //
        public string? SelectedSupplier { get; set; }

        // =====================================================================
        // 📦 Nabídka materiálu
        // =====================================================================
        //
        // Textový popis konkrétní nabídky dodavatele.
        // Např.: "ABB CHRÁNIČ PROUD. FH202 AC-40/0,03 2P 40A 30mA TYP AC 6KA"
        //
        public string? SelectedOffer { get; set; }

        // =====================================================================
        // 💰 Cena materiálu (volitelné)
        // =====================================================================
        //
        // Cena materiálu v době uložení projektu.
        // Pokud je null → cena se po načtení dopočítá z databáze.
        //
        public decimal? SelectedMaterialPrice { get; set; }

        // =====================================================================
        // 📏 Jednotka materiálu (volitelné)
        // =====================================================================
        //
        // Jednotka materiálu (např. "ks", "m", "bm").
        // Pokud je null → jednotka se po načtení dohledá z databáze.
        //
        public string? SelectedMaterialUnit { get; set; }
    }
}
