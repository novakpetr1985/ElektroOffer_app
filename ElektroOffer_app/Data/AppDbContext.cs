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
    // 🔴 ZMĚNA (1.9.0 — New Work Cascade):
    // - Odstraněn DbSet<PriceItems> (stará kaskáda PRÁCE, celá tabulka
    //   PriceItems zanikla, nenahrazuje se ničím zpětně kompatibilním)
    // - Přidány nové DbSety pro PRÁCI: WorkTask, WorkSpecification,
    //   BaseMaterial, WorkPosition, TaskSpecification
    // - Entitní třídy mají jiný název než DB tabulky (WorkTask → "Tasks"
    //   atd.), aby nekolidovaly s .NET typy (Task) nebo s vlastnostmi
    //   jinde v projektu (Position) – mapování řeší ToTable() níže.
    // - Sekce MATERIÁL (Materials, Categories, Suppliers, MaterialPrices)
    //   beze změny.
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
        // DB SETS – MATERIÁL (beze změny)
        // =========================================================================

        public DbSet<Material> Materials => Set<Material>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<MaterialPrice> MaterialPrices => Set<MaterialPrice>();

        // =========================================================================
        // DB SETS – PRÁCE (nová kaskáda 1.9.0)
        // =========================================================================

        /// <summary>Úkony (tabulka "Tasks"). Základní cena práce.</summary>
        public DbSet<WorkTask> Tasks => Set<WorkTask>();

        /// <summary>Specifikace (tabulka "Specifications"). Jen pro Unit + omezení kaskády.</summary>
        public DbSet<WorkSpecification> Specifications => Set<WorkSpecification>();

        /// <summary>Podklady (tabulka "BaseMaterials"). Koeficient materiálu.</summary>
        public DbSet<BaseMaterial> BaseMaterials => Set<BaseMaterial>();

        /// <summary>Pozice (tabulka "Positions"). Koeficient polohy.</summary>
        public DbSet<WorkPosition> Positions => Set<WorkPosition>();

        /// <summary>Validní páry Task ↔ Specification (tabulka "TaskSpecifications").</summary>
        public DbSet<TaskSpecification> TaskSpecifications => Set<TaskSpecification>();

        // =========================================================================
        // KONFIGURACE DB
        // =========================================================================

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;

            optionsBuilder.UseSqlite("Data Source=elektrooffer.db");
        }

        // =========================================================================
        // 🔧 OnModelCreating – dodatečná pravidla databázového schématu
        // =========================================================================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- MATERIÁL (beze změny) ----
            modelBuilder.Entity<MaterialPrice>()
                .HasIndex(mp => new { mp.SupplierId, mp.SupplierCode })
                .IsUnique();

            modelBuilder.Entity<MaterialPrice>()
                .Property(mp => mp.Price)
                .HasColumnType("REAL");

            // ---- PRÁCE (1.9.0) ----
            // Mapování entitních tříd na existující názvy DB tabulek
            modelBuilder.Entity<WorkTask>().ToTable("Tasks");
            modelBuilder.Entity<WorkSpecification>().ToTable("Specifications");
            modelBuilder.Entity<BaseMaterial>().ToTable("BaseMaterials");
            modelBuilder.Entity<WorkPosition>().ToTable("Positions");
            modelBuilder.Entity<TaskSpecification>().ToTable("TaskSpecifications");

            // Stejný pár Task+Specification nesmí být v tabulce dvakrát
            modelBuilder.Entity<TaskSpecification>()
                .HasIndex(ts => new { ts.TaskId, ts.SpecificationId })
                .IsUnique();

            // SQLite nemá nativní DECIMAL → explicitně REAL, stejně jako u MaterialPrice.Price
            modelBuilder.Entity<WorkTask>().Property(t => t.BasePrice).HasColumnType("REAL");
            modelBuilder.Entity<BaseMaterial>().Property(b => b.BaseMaterialCoef).HasColumnType("REAL");
            modelBuilder.Entity<WorkPosition>().Property(p => p.PositionCoef).HasColumnType("REAL");
        }
    }
}