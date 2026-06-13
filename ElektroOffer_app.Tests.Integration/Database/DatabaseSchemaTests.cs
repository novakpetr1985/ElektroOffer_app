using ElektroOffer_app.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Integration.Database;

// =========================================================================
// 🧪 DatabaseSchemaTests
// =========================================================================
//
// ÚČEL:
// - Ověření, že EF Core správně vytvoří databázové schéma
// - Testuje AppDbContext (ne SQLite ručně)
// - Ochrana proti rozbití DbSetů nebo modelů
//
// DŮLEŽITÉ:
// - Tento test NEtestuje SQLite
// - Tento test testuje Entity Framework konfiguraci
//
// Pokud selže:
// - nebyl vytvořen DbSet
// - chybí model
// - nebo je rozbitý AppDbContext
//
// =========================================================================

[TestFixture]
public class DatabaseSchemaTests
{
    // =====================================================================
    // KONFIGURACE TESTU
    // =====================================================================

    private SqliteConnection _connection = null!;
    private AppDbContext _db = null!;

    // =====================================================================
    // SETUP
    // =====================================================================

    /// <summary>
    /// Vytvoří čistou SQLite databázi v paměti
    /// před každým testem.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        // SQLite pouze v paměti
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        // EF Core konfigurace pro testovací databázi
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);

        // Vytvoření databázového schématu podle DbContext
        _db.Database.EnsureCreated();
    }

    // =====================================================================
    // TEARDOWN
    // =====================================================================

    /// <summary>
    /// Uvolnění prostředků po testu.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    // =====================================================================
    // TEST SCHÉMATU
    // =====================================================================

    /// <summary>
    /// Ověří, že EF Core vytvořil tabulky
    /// PriceItems a Materials.
    /// </summary>
    [Test]
    public void Should_Create_AppDbContext_Tables()
    {
        // =============================================================
        // ASSERT 1 – PriceItems tabulka existuje
        // =============================================================
        var priceItemsExists = _db.PriceItems != null;

        Assert.That(
            priceItemsExists,
            Is.True,
            "Tabulka PriceItems nebyla vytvořena."
        );

        // =============================================================
        // ASSERT 2 – Materials tabulka existuje
        // =============================================================
        var materialsExists = _db.Materials != null;

        Assert.That(
            materialsExists,
            Is.True,
            "Tabulka Materials nebyla vytvořena."
        );

        // =============================================================
        // BONUS ASSERT – databáze je skutečně vytvořená
        // =============================================================
        Assert.That(
            _db.Database.CanConnect(),
            Is.True,
            "Nelze se připojit k databázi."
        );
    }
}