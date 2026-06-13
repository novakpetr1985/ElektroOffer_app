using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Integration.Database;

// =========================================================================
// 🧪 DatabaseCrudTests
// =========================================================================
//
// ÚČEL:
// - Ověření základních CRUD operací nad AppDbContext
// - Testuje skutečnou databázovou vrstvu aplikace
// - Používá SQLite InMemory databázi
//
// CRUD:
// C = Create
// R = Read
// U = Update
// D = Delete
//
// PROČ:
// Pokud některá z těchto operací přestane fungovat,
// aplikace nebude schopná správně pracovat s databází.
//
// =========================================================================

[TestFixture]
public class DatabaseCrudTests
{
    // =====================================================================
    // TESTOVACÍ INFRASTRUKTURA
    // =====================================================================

    private SqliteConnection _connection = null!;
    private AppDbContext _db = null!;

    // =====================================================================
    // SETUP
    // =====================================================================

    /// <summary>
    /// Spustí se před každým testem.
    /// Vytvoří novou čistou SQLite databázi v paměti.
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

        // vytvoření tabulek dle AppDbContext
        _db.Database.EnsureCreated();
    }

    // =====================================================================
    // TEARDOWN
    // =====================================================================

    /// <summary>
    /// Úklid po každém testu.
    /// Uzavře databázi a uvolní prostředky.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    // =====================================================================
    // CREATE
    // =====================================================================

    /// <summary>
    /// Ověří, že lze vložit novou položku
    /// do tabulky PriceItems.
    /// </summary>
    [Test]
    public void Should_Insert_PriceItem()
    {
        var item = new PriceItems
        {
            Task = "Montáž zásuvky",
            Specification = "Standard",
            Material = "Cihla",
            Location = "Stěna",
            BasePrice = 100,
            MaterialCoef = 1,
            PositionCoef = 1,
            Unit = "ks"
        };

        _db.PriceItems.Add(item);

        _db.SaveChanges();

        Assert.That(
            _db.PriceItems.Count(),
            Is.EqualTo(1),
            "Položka nebyla vložena do databáze."
        );
    }

    // =====================================================================
    // READ
    // =====================================================================

    /// <summary>
    /// Ověří, že lze načíst uloženou položku
    /// z databáze.
    /// </summary>
    [Test]
    public void Should_Read_PriceItem()
    {
        var item = new PriceItems
        {
            Task = "Revize",
            BasePrice = 500
        };

        _db.PriceItems.Add(item);

        _db.SaveChanges();

        var loadedItem = _db.PriceItems.First();

        Assert.Multiple(() =>
        {
            Assert.That(
                loadedItem.Task,
                Is.EqualTo("Revize")
            );

            Assert.That(
                loadedItem.BasePrice,
                Is.EqualTo(500)
            );
        });
    }

    // =====================================================================
    // UPDATE
    // =====================================================================

    /// <summary>
    /// Ověří, že lze upravit existující položku.
    /// </summary>
    [Test]
    public void Should_Update_PriceItem()
    {
        var item = new PriceItems
        {
            Task = "Montáž",
            BasePrice = 100
        };

        _db.PriceItems.Add(item);

        _db.SaveChanges();

        item.BasePrice = 750;

        _db.SaveChanges();

        var updatedItem = _db.PriceItems.First();

        Assert.That(
            updatedItem.BasePrice,
            Is.EqualTo(750),
            "Změna ceny nebyla uložena."
        );
    }

    // =====================================================================
    // DELETE
    // =====================================================================

    /// <summary>
    /// Ověří, že lze odstranit položku
    /// z databáze.
    /// </summary>
    [Test]
    public void Should_Delete_PriceItem()
    {
        var item = new PriceItems
        {
            Task = "Test"
        };

        _db.PriceItems.Add(item);

        _db.SaveChanges();

        _db.PriceItems.Remove(item);

        _db.SaveChanges();

        Assert.That(
            _db.PriceItems.Count(),
            Is.EqualTo(0),
            "Položka nebyla odstraněna."
        );
    }
}