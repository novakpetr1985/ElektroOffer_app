
namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 🏷️ Category – kategorie materiálu (např. Kabely, Jističe, Chrániče)
    // =========================================================================
    //
    // K čemu slouží:
    // - Reprezentuje jeden záznam v tabulce Categories (SQLite)
    // - Používá se v AppDbContext (DbSet<Category>)
    // - Slouží čistě k ORGANIZACI materiálů v UI (filtrování, přehlednost),
    //   nemá žádný vliv na výpočet ceny ani na párování dodavatelů
    // - Jeden Material patří max. do jedné kategorie (nepovinné - viz
    //   CategoryId jako "int?" v Material.cs)
    //
    // Vlastnosti:
    // - Id        → primární klíč v databázi
    // - Name      → název kategorie (např. "Kabely", "Jističe")
    // - Materials → navigační vlastnost, seznam VŠECH materiálů spadajících
    //               do této kategorie (EF Core ji naplní automaticky při
    //               dotazu s .Include(c => c.Materials))
    // =========================================================================
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ICollection<Material> Materials { get; set; } = new List<Material>();
    }
}