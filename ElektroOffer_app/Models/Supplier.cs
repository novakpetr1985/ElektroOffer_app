namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 🚚 Supplier – dodavatel materiálu (např. ELKOV, EMAS)
    // =========================================================================
    //
    // K čemu slouží:
    // - Reprezentuje jeden záznam v tabulce Suppliers (SQLite)
    // - Používá se v AppDbContext (DbSet<Supplier>)
    // - Jeden Supplier může nabízet cenu na libovolné množství materiálů,
    //   proto má kolekci Prices (vazba 1:N na MaterialPrice)
    // - V DB budou zpočátku jen 2 záznamy (ELKOV, EMAS), ale struktura
    //   umožňuje přidat libovolný počet dalších dodavatelů bez zásahu
    //   do schématu databáze
    //
    // Vlastnosti:
    // - Id     → primární klíč v databázi
    // - Name   → název dodavatele (např. "ELKOV")
    // - Prices → navigační vlastnost, seznam VŠECH cen, které tento
    //            dodavatel nabízí napříč všemi materiály
    //            (ICollection<MaterialPrice>)
    // =========================================================================
    public class Supplier
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ICollection<MaterialPrice> Prices { get; set; } = new List<MaterialPrice>();
    }
}