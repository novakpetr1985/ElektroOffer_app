using Microsoft.EntityFrameworkCore;
using ElektroOffer_app.Models;

namespace ElektroOffer_app.Data
{
    // =========================================================================
    // 🧠 AppDbContext – EF Core databázový kontext
    // =========================================================================
    //
    // ÚČEL:
    // - Zprostředkovává komunikaci mezi aplikací a SQLite databází
    // - Nahrazuje ruční psaní SQL dotazů (SELECT, INSERT, UPDATE, DELETE)
    //
    // DŮLEŽITÉ:
    // - Musí podporovat dva režimy:
    //      1) Běžný provoz aplikace (SQLite soubor elektrooffer.db)
    //      2) Unit testy (SQLite InMemory přes DbContextOptions)
    //
    // PROČ JSOU DVA KONSTRUKTORY:
    // - AppDbContext(DbContextOptions<AppDbContext> options)
    //      → používají testy a DI kontejnery
    // - AppDbContext()
    //      → používá aplikace, pokud není DI
    //
    // OnConfiguring():
    // - Použije se pouze tehdy, pokud options NEJSOU nastavené
    //   (tj. v aplikaci ano, v testech ne)
    // =========================================================================
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Konstruktor používaný v testech a DI kontejnerech.
        /// Umožňuje předat vlastní DbContextOptions (např. SQLite InMemory).
        /// </summary>
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            // Tento konstruktor se používá v testech.
            // OnConfiguring se NEVOLÁ, protože options jsou již nastavené.
        }

        /// <summary>
        /// Konstruktor používaný aplikací, pokud není použit DI kontejner.
        /// </summary>
        public AppDbContext()
        {
            // Tento konstruktor se používá v běžném provozu aplikace.
            // OnConfiguring nastaví SQLite soubor elektrooffer.db.
        }

        // =========================================================================
        // DB SETS – tabulky v databázi
        // =========================================================================

        /// <summary>
        /// Tabulka ceníku práce (PriceItems).
        /// Každý záznam reprezentuje jednu položku ceníku.
        /// </summary>
        public DbSet<PriceItems> PriceItems => Set<PriceItems>();

        /// <summary>
        /// Tabulka ceníku materiálu (Materials).
        /// Každý záznam reprezentuje jeden materiál.
        /// </summary>
        public DbSet<Material> Materials => Set<Material>();

        // =========================================================================
        // KONFIGURACE DB
        // =========================================================================

        /// <summary>
        /// Konfigurace databázového připojení.
        /// Použije se pouze tehdy, pokud nebyly předány DbContextOptions.
        /// (např. v testech se NEPOUŽIJE)
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Pokud už jsou options nastavené (např. testy), nic nedělej
            if (optionsBuilder.IsConfigured)
                return;

            // Výchozí konfigurace pro aplikaci – SQLite soubor elektrooffer.db
            optionsBuilder.UseSqlite("Data Source=elektrooffer.db");
        }
    }
}
