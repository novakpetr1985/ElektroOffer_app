﻿namespace ElektroOffer_app.Models
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
    // Co se ukládá:
    // --------------
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
