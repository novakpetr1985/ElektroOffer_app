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
    //
    // NOVĚ (multi-dodavatelské ceny materiálu):
    // - Přibyly tabulky Categories, Suppliers a MaterialPrices – viz
    //   podrobný popis u jednotlivých DbSet vlastností níže
    // - Přibyla metoda OnModelCreating(), která definuje DODATEČNÁ
    //   pravidla databázového schématu, jež nejdou vyjádřit pouze
    //   vlastnostmi na entitních třídách (konkrétně unikátní index
    //   proti duplicitním cenám od stejného dodavatele)
    //
    // 🔴 ZMĚNA (kaskáda Práce – normalizace podle vzoru Materiálu):
    // - Přibyly tabulky WorkTasks, WorkSpecifications, BaseMaterials,
    //   Positions a vazební TaskSpecifications – viz popis u DbSet
    //   vlastností níže.
    // - Stará tabulka PriceItems ZŮSTÁVÁ (viz komentář u DbSet PriceItems
    //   níže) – prozatím se řeší, zda a jak se z ní data migrují.
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
        ///
        /// 🔴 ZMĚNA / STATUS: Zatím ZACHOVÁNO souběžně s novou normalizovanou
        /// kaskádou (WorkTasks/WorkSpecifications/BaseMaterials/Positions/
        /// TaskSpecifications) – řešíme, zda se má obsah migrovat a tato
        /// tabulka následně zrušit, nebo zda má sloužit jinému účelu.
        /// Než se rozhodne, NEPOUŽÍVAT pro novou kaskádu Práce v UI.
        /// </summary>
        public DbSet<PriceItems> PriceItems => Set<PriceItems>();

        /// <summary>
        /// Tabulka ceníku materiálu (Materials).
        /// Každý záznam reprezentuje jeden materiál.
        ///
        /// NOVĚ: Material je nyní "partial" třída rozdělená do dvou
        /// souborů (Material.cs + Materials.cs). Druhá část přidává
        /// vazbu na Category a kolekci MaterialPrice (ceny od
        /// jednotlivých dodavatelů) – viz komentáře v Materials.cs.
        /// </summary>
        public DbSet<Material> Materials => Set<Material>();

        /// <summary>
        /// Tabulka kategorií materiálu (Categories).
        /// Každý záznam reprezentuje jednu kategorii (např. "Kabely",
        /// "Jističe", "Chrániče"). Slouží čistě k organizaci a
        /// filtrování materiálů v UI – nemá vliv na ceny ani párování
        /// dodavatelů. Jeden Material patří max. do jedné kategorie
        /// (nepovinné - viz Material.CategoryId jako "int?").
        /// </summary>
        public DbSet<Category> Categories => Set<Category>();

        /// <summary>
        /// Tabulka dodavatelů (Suppliers).
        /// Každý záznam reprezentuje jednoho dodavatele materiálu
        /// (např. "ELKOV", "EMAS"). Jeden Supplier může nabízet cenu
        /// na libovolné množství materiálů (vazba 1:N na MaterialPrice).
        /// </summary>
        public DbSet<Supplier> Suppliers => Set<Supplier>();

        /// <summary>
        /// Tabulka cen materiálu od jednotlivých dodavatelů (MaterialPrices).
        /// Každý záznam reprezentuje cenu JEDNOHO materiálu OD JEDNOHO
        /// konkrétního dodavatele – řeší vztah M:N mezi Material a
        /// Supplier (jeden materiál může mít víc dodavatelů, jeden
        /// dodavatel nabízí víc materiálů) a navíc nese vlastní data
        /// (cenu, kód a název položky u dodavatele, jednotku, měnu,
        /// datum poslední aktualizace).
        ///
        /// Viz unikátní index v OnModelCreating() níže – zabraňuje
        /// duplicitě záznamu se stejným kódem od stejného dodavatele.
        /// </summary>
        public DbSet<MaterialPrice> MaterialPrices => Set<MaterialPrice>();

        // =========================================================================
        // 🔴 NOVÉ DB SETS – normalizovaná kaskáda PRÁCE (analogie k Materiálu)
        // =========================================================================

        /// <summary>
        /// Úkony práce (dřív string "Task" v PriceItems, např. "Drážkování",
        /// "Osazení"). Nese základní cenu (BasePrice), která se dále násobí
        /// koeficienty BaseMaterial a Position.
        /// Prvotní úroveň kaskády Práce – nabízí se vždy celý seznam.
        /// </summary>
        public DbSet<WorkTask> WorkTasks => Set<WorkTask>();

        /// <summary>
        /// Upřesnění úkonu (dřív string "Specification" v PriceItems,
        /// např. "El. krabice", "Spára"). Druhá úroveň kaskády – filtruje
        /// se podle vybraného WorkTask přes vazební TaskSpecifications.
        /// </summary>
        public DbSet<WorkSpecification> WorkSpecifications => Set<WorkSpecification>();

        /// <summary>
        /// Podkladový materiál (Beton, Cihla, Sádrokarton...), ovlivňuje
        /// cenu práce koeficientem MaterialCoef. POZOR: nesouvisí
        /// s produktovým materiálem (Models/Material.cs) – jde o odlišný,
        /// nezávislý koncept se stejnojmenným slovem "materiál".
        /// Nabízí se vždy celý seznam, nezávisle na Task/Specification.
        /// </summary>
        public DbSet<BaseMaterial> BaseMaterials => Set<BaseMaterial>();

        /// <summary>
        /// Poloha provádění práce (Nízká, Strop, Stěna, Štafle), ovlivňuje
        /// cenu práce koeficientem PositionCoef.
        /// Nabízí se vždy celý seznam, nezávisle na Task/Specification.
        /// </summary>
        public DbSet<Position> Positions => Set<Position>();

        /// <summary>
        /// Vazební (M:N) tabulka mezi WorkTask a WorkSpecification – určuje,
        /// které kombinace jsou PLATNÉ (obdoba role MaterialPrice mezi
        /// Material a Supplier, ale bez vlastních cenových dat – cena
        /// práce se počítá vzorcem, nikoli lookupem).
        ///
        /// Viz unikátní index v OnModelCreating() níže – zabraňuje
        /// duplicitní platné kombinaci.
        /// </summary>
        public DbSet<TaskSpecification> TaskSpecifications => Set<TaskSpecification>();

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

        // =========================================================================
        // 🔧 OnModelCreating – dodatečná pravidla databázového schématu
        // =========================================================================
        //
        // K ČEMU SLOUŽÍ:
        // - EF Core zde umožňuje doladit schéma databáze způsobem,
        //   který nejde (nebo by byl nepřehledný) zapsat přímo jako
        //   datovou anotaci na vlastnosti entitní třídy (např. [Required]).
        // - Aktuálně obsahuje pravidlo:
        //
        // UNIKÁTNÍ INDEX na MaterialPrice (SupplierId + SupplierCode):
        // - Zabraňuje, aby od JEDNOHO DODAVATELE vznikly DVA záznamy
        //   MaterialPrice se STEJNÝM kódem položky (SupplierCode).
        // - Díky tomu může import ceníku (viz MaterialImportService)
        //   bezpečně fungovat jako "upsert": nejdřív se podle dvojice
        //   (SupplierId, SupplierCode) zkusí najít existující záznam,
        //   a pokud existuje, jen se aktualizuje cena. Pokud databáze
        //   umožňovala duplicity, mohl by se při opakovaném importu
        //   stejný kód uložit vícekrát a vznikl by nekonzistentní stav
        //   (dvě různé "aktuální" ceny pro stejnou položku).
        //
        // 🔴 ZMĚNA – UNIKÁTNÍ INDEX na TaskSpecification (TaskId + SpecificationId):
        // - Stejný princip jako u MaterialPrice výše, ale pro platné
        //   kombinace Úkon+Upřesnění u kaskády Práce.
        // =========================================================================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MaterialPrice>()
                .HasIndex(mp => new { mp.SupplierId, mp.SupplierCode })
                .IsUnique();

            // -------------------------------------------------------------------------
            // 💡 DODATEČNÁ KONFIGURACE PRO SQLITE
            // -------------------------------------------------------------------------
            // PROBLÉM:
            // - SQLite nemá nativní typ DECIMAL, ukládá hodnoty jako TEXT (např. "10.0")
            // - EF Core se pak při načítání pokouší převést text na decimal → FormatException
            //
            // ŘEŠENÍ:
            // - Explicitně nastavíme typ sloupce Price na REAL
            //   → SQLite uloží hodnotu jako číslo (REAL), ne jako text
            //   → decimal se načte správně
            //   → testy (např. T_43) přestanou padat
            //
            // DŮSLEDEK:
            // - Aplikace i testy fungují beze změny logiky
            // - Žádné dopady na importy, výpočty ani datové modely
            // -------------------------------------------------------------------------
            modelBuilder.Entity<MaterialPrice>()
                .Property(mp => mp.Price)
                .HasColumnType("REAL");

            // -------------------------------------------------------------------------
            // 🔴 NOVÉ – TaskSpecification (vazební tabulka kaskády Práce)
            // -------------------------------------------------------------------------
            modelBuilder.Entity<TaskSpecification>()
                .HasIndex(ts => new { ts.TaskId, ts.SpecificationId })
                .IsUnique();

            // 🔴 NOVÉ – WorkTask.BasePrice je decimal → stejný SQLite problém
            // jako u MaterialPrice.Price výše, řešíme stejným způsobem.
            modelBuilder.Entity<WorkTask>()
                .Property(t => t.BasePrice)
                .HasColumnType("REAL");
        }

    }
}