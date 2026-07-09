namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 💰 Material (rozšíření) – vazba na kategorii a ceny od více dodavatelů
    // =========================================================================
    //
    // K čemu slouží:
    // - Toto je DRUHÁ ČÁST třídy Material (viz "partial" u definice třídy
    //   v Material.cs). Hlavní část třídy (Id, Name, Price, Unit) zůstává
    //   beze změny v souboru Material.cs – tento soubor jen DOPLŇUJE
    //   stejnou třídu o nové vlastnosti.
    //
    // Nové vlastnosti:
    // - CategoryId → cizí klíč na Category (nepovinné, proto "int?")
    // - Category   → navigační vlastnost k celému objektu Category
    // - Prices     → navigační vlastnost, seznam VŠECH cen tohoto
    //                materiálu od různých dodavatelů
    // =========================================================================
    public partial class Material
    {
        public int? CategoryId { get; set; }

        public Category? Category { get; set; }

        public ICollection<MaterialPrice> Prices { get; set; } = new List<MaterialPrice>();
    }
}