using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using Microsoft.EntityFrameworkCore;

namespace ElektroOffer_app.Services
{
    // =========================================================================
    // 🗂️ CatalogService – načítání dat ceníku z databáze
    // =========================================================================
    //
    // ÚČEL:
    // - Odděluje logiku načítání ceníku od UI (MainWindow)
    // - Umožňuje testovat načítání dat bez WPF a bez MainWindow
    // - Přijímá AppDbContext zvenčí → v testech lze předat testovací DB
    //
    // PROČ TATO TŘÍDA EXISTUJE:
    // - Dříve byla logika přímo v MainWindow.LoadCatalogDataFromDb()
    // - To znemožňovalo integrační testování bez spuštění WPF okna
    // - CatalogService tuto logiku přebírá a MainWindow jen volá výsledek
    //
    // POUŽITÍ V APLIKACI:
    //   var service = new CatalogService();
    //   var (tasks, materials) = service.LoadCatalog(new AppDbContext());
    //
    // POUŽITÍ V TESTECH:
    //   var options = new DbContextOptionsBuilder<AppDbContext>()
    //       .UseSqlite("Data Source=:memory:")
    //       .Options;
    //   using var db = new AppDbContext(options);
    //   db.Database.EnsureCreated();
    //   var service = new CatalogService();
    //   var (tasks, materials) = service.LoadCatalog(db);
    //
    // =========================================================================
    public class CatalogService
    {
        // =====================================================================
        // HLAVNÍ: Načtení ceníku z databáze
        // =====================================================================

        /// <summary>
        /// Načte seznam úkonů (Tasks) a materiálů (Materials) z databáze.
        /// </summary>
        /// <param name="db">
        /// Instance AppDbContext — předána zvenčí, aby bylo možné
        /// v testech použít jinou (in-memory) databázi.
        /// </param>
        /// <returns>
        /// Tuple obsahující:
        /// - Tasks: seznam unikátních názvů úkonů z ceníku práce
        /// - Materials: seznam všech materiálů z ceníku materiálu
        /// </returns>
        public (List<string> Tasks, List<Material> Materials) LoadCatalog(AppDbContext db)
        {
            // ------------------------------------------------------------------
            // Načtení unikátních úkonů z tabulky PriceItems
            // ------------------------------------------------------------------
            // Select(x => x.Task)  → vezme jen sloupec Task
            // Distinct()           → odstraní duplicity (stejný úkon na více řádcích)
            // ToList()             → spustí SQL dotaz a vrátí výsledek jako List
            // ------------------------------------------------------------------
            var tasks = db.PriceItems
                .Select(x => x.Task)
                .Distinct()
                .ToList();

            // ------------------------------------------------------------------
            // Načtení všech materiálů z tabulky Materials
            // ------------------------------------------------------------------
            // ToList() spustí: SELECT * FROM Materials
            // ------------------------------------------------------------------
            var materials = db.Materials
                .ToList();

            // ------------------------------------------------------------------
            // Vrácení obou výsledků jako tuple (dvojice hodnot)
            // ------------------------------------------------------------------
            // Volající (MainWindow nebo test) si výsledek rozbalí takto:
            //   var (tasks, materials) = service.LoadCatalog(db);
            // ------------------------------------------------------------------
            return (tasks, materials);
        }

        // =====================================================================
        // VEDLEJŠÍ: Kontrola, zda je ceník prázdný
        // =====================================================================

        /// <summary>
        /// Vrátí true, pokud databáze neobsahuje žádné položky ceníku.
        /// Používá se při prvním spuštění pro rozhodnutí, zda načíst seed data.
        /// </summary>
        /// <param name="db">Instance AppDbContext předaná zvenčí.</param>
        public bool IsCatalogEmpty(AppDbContext db)
        {
            // Any() vrátí true, pokud existuje alespoň jeden záznam
            // !Any() → žádný záznam = ceník je prázdný
            return !db.PriceItems.Any() && !db.Materials.Any();
        }
    }
}