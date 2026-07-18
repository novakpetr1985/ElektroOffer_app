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
    // Každý uložený řádek MATERIÁLU má vlastní sekvenční identifikátor.
    // Stejné ID se ukládá i do odpovídající položky CalculationItemData.
    //
    // Díky tomu lze jednoznačně spárovat:
    //
    //      MaterialItemData.Id == CalculationItemData.Id
    //
    // To je zásadní pro stabilní Load/Save a pro budoucí rozšiřování projektu.
    //
    // Id vs. Position:
    // ----------------------------------------------------------------
    // Id a Position mají nyní odlišný účel:
    //
    //   • Id        → jednoznačný, SEKVENČNÍ identifikátor záznamu
    //                  (M-1, M-2, M-3... podle pořadí mezi vyplněnými řádky).
    //                  Používá se výhradně pro párování se CalculationItemData.
    //                  Nemění se podle pozice řádku v UI.
    //
    //   • Position  → skutečná POZICE řádku v UI (1-based), nezávislá na Id.
    //                  Používá se výhradně pro znovuvytvoření správného
    //                  rozložení řádků při načítání projektu (ApplyProjectData).
    //
    // Dříve se pozice řádku odvozovala parsováním čísla z Id (např. "M-5" → 5),
    // což fungovalo jen do doby, než Id přestalo odpovídat pozici (např. pokud
    // je vyplněný jen 1. a 5. řádek, druhý vyplněný záznam by měl Id "M-2",
    // ale pozici 5). Oddělením obou hodnot je Id stabilní a smysluplné
    // („druhý vyplněný záznam“) a Position spolehlivě řídí rozložení v UI.
    //
    // Co se ukládá:
    // --------------
    // ✔ Id                     → jednoznačný, sekvenční identifikátor záznamu
    // ✔ Position                → skutečná pozice řádku v UI (1-based)
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
        // 🆔 Id – jednoznačný, sekvenční identifikátor řádku MATERIÁLU
        // =====================================================================
        //
        // ID je typu string, protože používáme krátké lidsky čitelné ID:
        //   • M-1, M-2, M-3...
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
        // řádku v kolekci (MaterialItems) v okamžiku uložení, a to ještě
        // před odfiltrováním prázdných řádků.
        //
        // Např.: pokud uživatel vyplní jen 1. a 5. řádek, uloží se:
        //   • 1. řádek → Id = "M-1", Position = 1
        //   • 5. řádek → Id = "M-2", Position = 5
        //
        // Při načítání projektu (ApplyProjectData) se řádek vkládá na
        // index = Position - 1, takže se obnoví přesně původní rozložení
        // řádků včetně mezer mezi vyplněnými řádky.
        //
        public int Position { get; set; }

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
