﻿namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 💰 Material (rozšíření) – vazba na kategorii a ceny od více dodavatelů
    // =========================================================================
    //
    // K čemu slouží:
    // - Toto je DRUHÁ ČÁST třídy Material (viz "partial" u definice třídy
    //   níže). Hlavní část třídy (Id, Name, Price, Unit) zůstává beze
    //   změny v souboru Material.cs – tento soubor jen DOPLŇUJE stejnou
    //   třídu o nové vlastnosti, aniž by bylo nutné Material.cs upravovat
    // - Klíčové slovo "partial" u třídy říká kompilátoru, ať obě části
    //   (z Material.cs i z tohoto souboru) spojí dohromady do jedné
    //   třídy, jako by byly napsané v jednom souboru
    //
    // Proč tato změna vznikla:
    // - Původně měl každý Material jen jednu jedinou cenu (Price ve staré
    //   části třídy). To ale neumožňovalo mít u stejného materiálu ceny
    //   od dvou a více dodavatelů (např. ELKOV a EMAS)
    // - Řešením je nová tabulka MaterialPrices (viz MaterialPrice.cs),
    //   kde je pro každou dvojici Material + Supplier uložená samostatná
    //   cena. Jeden Material tak může mít libovolný počet souvisejících
    //   záznamů MaterialPrice (1 pro každého dodavatele, který ho nabízí)
    //
    // Nové vlastnosti:
    // - CategoryId → cizí klíč na Category (nepovinné, proto "int?").
    //                Umožňuje zařadit materiál do kategorie
    //                (např. "Kabely", "Jističe") pro přehlednější
    //                filtrování a orientaci v seznamu materiálů
    // - Category   → navigační vlastnost k celému objektu Category
    //                (EF Core podle CategoryId sám dohledá odpovídající
    //                záznam, pokud si ho vyžádáš přes
    //                .Include(m => m.Category))
    // - Prices     → navigační vlastnost, seznam VŠECH cen tohoto
    //                materiálu od různých dodavatelů
    //                (ICollection<MaterialPrice>). Např.
    //                material.Prices[0].Supplier.Name vrátí jméno
    //                prvního dodavatele, material.Prices[0].Price jeho cenu
    //
    // Poznámka k Price ve staré (hlavní) části třídy:
    // - Původní vlastnost Price v Material.cs prozatím ZŮSTÁVÁ zachovaná,
    //   i když nová logika cen žije v MaterialPrice. Důvod: části
    //   aplikace (např. CalculationItemViewModel), které dosud čtou
    //   material.Price přímo, by se jinak přestaly kompilovat/fungovat.
    //   Až budou všechna místa v kódu přepnutá na čtení ceny přes
    //   MaterialPrice (podle vybraného dodavatele), půjde staré pole
    //   Price bezpečně odstranit v samostatné, pozdější verzi
    //   (úklidový patch)
    // =========================================================================
    public partial class Material
    {
        public int? CategoryId { get; set; }

        public Category? Category { get; set; }

        public ICollection<MaterialPrice> Prices { get; set; } = new List<MaterialPrice>();
    }
}