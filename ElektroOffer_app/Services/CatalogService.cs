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
    // - Odděluje logiku načítání ceníku od UI (MainWindow / MainViewModel)
    // - Umožňuje testovat načítání dat bez WPF
    // - Přijímá AppDbContext zvenčí → v testech lze předat testovací DB
    //
    // 🔴 ZMĚNA (1.9.0 — New Work Cascade):
    // - Odstraněna veškerá logika nad starou tabulkou PriceItems
    // - Přidány metody pro novou kaskádu PRÁCE:
    //     GetWorkTasks()          → celý seznam úkonů (Tasks)
    //     GetWorkSpecifications()  → specifikace OMEZENÉ podle vybraného
    //                                WorkTask (přes TaskSpecifications)
    //     GetBaseMaterials()      → celý seznam podkladů (BaseMaterials)
    //     GetWorkPositions()      → celý seznam pozic (Positions)
    //
    // - Jediné omezení v kaskádě je Task → Specification (validní páry).
    //   BaseMaterial a WorkPosition se vždy nabízí kompletní, nezávisle
    //   na vybraném Tasku/Specifikaci.
    //
    // - Sekce MATERIÁL (LoadCatalog, IsCatalogEmpty pro Materials) beze změny.
    // =========================================================================
    public class CatalogService
    {
        // =====================================================================
        // MATERIÁL (beze změny)
        // =====================================================================

        /// <summary>
        /// Načte seznam materiálů z databáze (sekce MATERIÁL).
        /// </summary>
        public List<Material> LoadMaterials(AppDbContext db)
        {
            return db.Materials.ToList();
        }

        // =====================================================================
        // PRÁCE (nová kaskáda 1.9.0)
        // =====================================================================

        /// <summary>
        /// Načte kompletní seznam úkonů (WorkTask). Nabízí se vždy celý,
        /// bez omezení – je to první krok kaskády.
        /// </summary>
        public List<WorkTask> GetWorkTasks(AppDbContext db)
        {
            return db.Tasks
                .OrderBy(t => t.Name)
                .ToList();
        }

        /// <summary>
        /// Načte specifikace VALIDNÍ pro daný WorkTask (podle tabulky
        /// TaskSpecifications). Toto je jediné omezení v celé kaskádě PRÁCE.
        /// </summary>
        /// <param name="taskId">Id vybraného WorkTask.</param>
        public List<WorkSpecification> GetWorkSpecifications(AppDbContext db, int taskId)
        {
            return db.TaskSpecifications
                .Where(ts => ts.TaskId == taskId)
                .Include(ts => ts.Specification)
                .Select(ts => ts.Specification!)
                .OrderBy(s => s.Name)
                .ToList();
        }

        /// <summary>
        /// Načte kompletní seznam podkladů (BaseMaterial). Není omezen
        /// podle Tasku ani Specifikace – nabízí se vždy celý.
        /// </summary>
        public List<BaseMaterial> GetBaseMaterials(AppDbContext db)
        {
            return db.BaseMaterials
                .OrderBy(b => b.Name)
                .ToList();
        }

        /// <summary>
        /// Načte kompletní seznam pozic (WorkPosition). Není omezen
        /// podle Tasku ani Specifikace – nabízí se vždy celý.
        /// </summary>
        public List<WorkPosition> GetWorkPositions(AppDbContext db)
        {
            return db.Positions
                .OrderBy(p => p.Name)
                .ToList();
        }

        // =====================================================================
        // VEDLEJŠÍ: Kontrola, zda je ceník prázdný
        // =====================================================================

        /// <summary>
        /// Vrátí true, pokud databáze neobsahuje žádné položky ceníku
        /// (ani práce, ani materiál). Používá se při prvním spuštění
        /// pro rozhodnutí, zda načíst seed data.
        /// </summary>
        public bool IsCatalogEmpty(AppDbContext db)
        {
            return !db.Tasks.Any() && !db.Materials.Any();
        }
    }
}