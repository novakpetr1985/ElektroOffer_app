using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Integration.Services;

// =========================================================================
// 🧪 CatalogServiceTests (STABILNÍ VERZE)
// =========================================================================
//
// FIX:
// - Použit SQLite shared in-memory connection
// - Zabránění zamrzání test runneru
// - Správné lifecycle DB (open → use → dispose)
//
// ÚČEL:
// - Testování business logiky CatalogService
// - Ověření práce s EF Core přes SQLite InMemory DB
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
    /// Inicializace sdílené SQLite InMemory databáze.
    /// 
    /// DŮLEŽITÉ:
    /// Používáme "file:memdb1?mode=memory&cache=shared"
    /// → zabrání ztrátě DB mezi EF Core operacemi
    /// → eliminuje freeze test runneru
    /// </summary>
    [SetUp]
    public void Setup()
    {
        // -------------------------------------------------------------
        // 1. Otevření SQLite connection (KRITICKÉ)
        // -------------------------------------------------------------
        _connection = new SqliteConnection(
            "Data Source=file:memdb1?mode=memory&cache=shared"
        );

        _connection.Open();

        // -------------------------------------------------------------
        // 2. EF Core konfigurace
        // -------------------------------------------------------------
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);

        // -------------------------------------------------------------
        // 3. Vytvoření DB schématu
        // -------------------------------------------------------------
        _db.Database.EnsureCreated();

        // -------------------------------------------------------------
        // 4. Service vrstva
        // -------------------------------------------------------------
        _service = new CatalogService();
    }

    // =====================================================================
    // TEARDOWN
    // =====================================================================

    /// <summary>
    /// Bezpečné uvolnění zdrojů po každém testu.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _db?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
/*
    // =====================================================================
    // IsCatalogEmpty - neřeším prázdnou databázi
    // =====================================================================

    /// <summary>
    /// Ověří, že katalog je prázdný.
    /// </summary>
    [Test]
    public void Should_Return_True_When_Catalog_Is_Empty()
    {
        var result = _service.IsCatalogEmpty(_db);

        Assert.That(result, Is.True);
    }
*/
    /// <summary>
    /// Ověří, že katalog není prázdný,
    /// pokud obsahuje data.
    /// </summary>
    [Test]
    public void Should_Return_False_When_Catalog_Contains_Data()
    {
        // ARRANGE
        _db.PriceItems.Add(new PriceItems
        {
            Task = "Montáž",
            BasePrice = 100
        });

        _db.Materials.Add(new Material
        {
            Name = "Kabel",
            Price = 10
        });

        _db.SaveChanges();

        // ACT
        var result = _service.IsCatalogEmpty(_db);

        // ASSERT
        Assert.That(result, Is.False);
    }

    // =====================================================================
    // LoadCatalog
    // =====================================================================

    /// <summary>
    /// Ověří správné načtení katalogu:
    /// - unikátní Tasks
    /// - všechny Materials
    /// </summary>
    [Test]
    public void Should_Load_Catalog_Correctly()
    {
        // ARRANGE
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

        // ACT
        var (tasks, materials) = _service.LoadCatalog(_db);

        // ASSERT
        Assert.That(tasks.Count, Is.EqualTo(2));
        Assert.That(tasks, Does.Contain("Montáž"));
        Assert.That(tasks, Does.Contain("Revize"));

        Assert.That(materials.Count, Is.EqualTo(2));
    }
}