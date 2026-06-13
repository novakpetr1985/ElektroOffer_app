using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Integration.Database;

// =========================================================================
// 🧪 CatalogServiceTests
// =========================================================================
//
// ÚČEL:
// - Ověření business logiky CatalogService
// - Testuje práci s AppDbContext přes service vrstvu
// - Ověřuje správné chování nad reálnou SQLite InMemory DB
//
// TESTOVANÉ FUNKCE:
// - LoadCatalog()
// - IsCatalogEmpty()
//
// DŮLEŽITÉ:
// - Tento test NEtestuje EF Core samotný
// - Tento test testuje service logiku nad databází
//
// =========================================================================

[TestFixture]
public class CatalogServiceTests
{
    // =====================================================================
    // TEST INFRASTRUKTURA
    // =====================================================================

    private SqliteConnection _connection = null!;
    private AppDbContext _db = null!;
    private CatalogService _service = null!;

    // =====================================================================
    // SETUP
    // =====================================================================

    /// <summary>
    /// Inicializace SQLite InMemory databáze
    /// a připravení testovacího prostředí.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        _service = new CatalogService();
    }

    // =====================================================================
    // TEARDOWN
    // =====================================================================

    /// <summary>
    /// Uvolnění paměti po testu.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    // =====================================================================
    // IsCatalogEmpty
    // =====================================================================

    /// <summary>
    /// Ověří, že service vrátí true,
    /// pokud je databáze prázdná.
    /// </summary>
    [Test]
    public void Should_Return_True_When_Catalog_Is_Empty()
    {
        // Act
        var result = _service.IsCatalogEmpty(_db);

        // Assert
        Assert.That(
            result,
            Is.True,
            "Očekává se prázdný katalog, ale výsledek je false."
        );
    }

    /// <summary>
    /// Ověří, že service vrátí false,
    /// pokud databáze obsahuje data.
    /// </summary>
    [Test]
    public void Should_Return_False_When_Catalog_Contains_Data()
    {
        // Arrange
        _db.PriceItems.Add(new PriceItems
        {
            Task = "Montáž zásuvky",
            BasePrice = 100
        });

        _db.Materials.Add(new Material
        {
            Name = "Kabel CYKY",
            Price = 10
        });

        _db.SaveChanges();

        // Act
        var result = _service.IsCatalogEmpty(_db);

        // Assert
        Assert.That(
            result,
            Is.False,
            "Očekává se neprázdný katalog, ale výsledek je true."
        );
    }

    // =====================================================================
    // LoadCatalog
    // =====================================================================

    /// <summary>
    /// Ověří, že LoadCatalog vrátí správně
    /// načtené materiály a unikátní Tasks.
    /// </summary>
    [Test]
    public void Should_Load_Catalog_Correctly()
    {
        // Arrange
        _db.PriceItems.AddRange(
            new PriceItems { Task = "Montáž", BasePrice = 100 },
            new PriceItems { Task = "Montáž", BasePrice = 200 },
            new PriceItems { Task = "Revize", BasePrice = 300 }
        );

        _db.Materials.AddRange(
            new Material { Name = "Kabel", Price = 10 },
            new Material { Name = "Jistič", Price = 50 }
        );

        _db.SaveChanges();

        // Act
        var (tasks, materials) = _service.LoadCatalog(_db);

        // Assert
        Assert.That(
            tasks.Count,
            Is.EqualTo(2),
            "Tasks musí být unikátní (Distinct)."
        );

        Assert.That(
            tasks,
            Does.Contain("Montáž"),
            "Chybí očekávaný Task 'Montáž'."
        );

        Assert.That(
            tasks,
            Does.Contain("Revize"),
            "Chybí očekávaný Task 'Revize'."
        );

        Assert.That(
            materials.Count,
            Is.EqualTo(2),
            "Materiály nebyly načteny správně."
        );
    }
}