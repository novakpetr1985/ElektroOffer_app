namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 💶 MaterialPrice – cena JEDNOHO materiálu OD JEDNOHO konkrétního dodavatele
    // =========================================================================
    //
    // K čemu slouží:
    // - Reprezentuje jeden záznam v tabulce MaterialPrices (SQLite)
    // - Používá se v AppDbContext (DbSet<MaterialPrice>)
    // - Je to tzv. spojovací (junction) tabulka mezi Material a Supplier –
    //   řeší vztah M:N (jeden materiál může mít víc dodavatelů, jeden
    //   dodavatel nabízí víc materiálů), a navíc nese vlastní data (cenu)
    // - Existuje unikátní omezení (viz AppDbContext) na dvojici
    //   SupplierId + SupplierCode, takže od jednoho dodavatele nemůže
    //   omylem vzniknout duplicitní záznam se stejným kódem
    //
    // Vlastnosti:
    // - Id           → primární klíč v databázi
    // - MaterialId   → cizí klíč na Material (na KTERÝ materiál se cena vztahuje)
    // - Material     → navigační vlastnost k celému objektu Material
    // - SupplierId   → cizí klíč na Supplier (OD KTERÉHO dodavatele cena je)
    // - Supplier     → navigační vlastnost k celému objektu Supplier
    // - SupplierCode → kód položky PŘESNĚ tak, jak ho používá dodavatel
    //                  ve svém ceníku (např. "17018" u ELKOV,
    //                  "ELKAOS0802380" u EMAS). Slouží jako stabilní klíč
    //                  při opakovaném importu ceníku - podle něj poznáme,
    //                  že jde o už dříve napárovanou položku, a jen
    //                  aktualizujeme cenu, místo abychom vytvářeli duplicitu
    // - SupplierName → přesný název položky z ceníku dodavatele (např.
    //                  "Kabel CYKY-J 3x1,5 (C)"). Uchováváme ho zvlášť od
    //                  Material.Name (což je NÁŠ vlastní, kanonický název),
    //                  protože se hodí při kontrole objednávky/faktury
    //                  vidět přesně to, co je psáno v ceníku dodavatele
    // - Unit         → měrná jednotka PODLE CENÍKU dodavatele (m, ks...).
    //                  Slouží jako kontrola shody s Material.Unit - pokud
    //                  by se lišily, znamená to špatné párování při importu
    // - Price        → cena za jednotku bez DPH od tohoto dodavatele
    // - Currency     → měna (aktuálně vždy "Kč", ale připraveno do budoucna)
    // - UpdatedAt    → kdy byla cena naposledy aktualizována (např. při
    //                  importu novějšího ceníku) - pro budoucí přehled
    //                  "jak stará je tahle cena"
    // =========================================================================
    public class MaterialPrice
    {
        public int Id { get; set; }

        public int MaterialId { get; set; }
        public Material Material { get; set; } = null!;

        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;

        public string SupplierCode { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "Kč";
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}