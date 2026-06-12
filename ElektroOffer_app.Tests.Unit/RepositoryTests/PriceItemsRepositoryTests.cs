using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ElektroOffer_app.Tests.Unit.RepositoryTests
{
    /// <summary>
    /// Testy EF Core DbContextu pro tabulku PriceItems.
    /// Používá se SQLite InMemory databáze, která se chová stejně jako reálná SQLite,
    /// ale je rychlá a izolovaná pro každý test.
    /// </summary>
    [TestFixture]
    public class PriceItemsRepositoryTests
    {
        /// <summary>
        /// Vytvoří nový EF Core kontext s InMemory SQLite databází.
        /// Každý test dostane vlastní izolovanou DB.
        /// </summary>
        private AppDbContext CreateInMemoryContext()
        {
            // SQLite InMemory databáze – existuje jen po dobu otevřeného připojení
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new AppDbContext(options);

            // vytvoření schématu (tabulek)
            context.Database.EnsureCreated();

            return context;
        }

        // ============================================================
        // TEST 1 – INSERT
        // ============================================================

        /// <summary>
        /// Ověří, že lze vložit novou položku PriceItems.
        /// </summary>
        [Test]
        public void Insert_Should_Add_New_PriceItem()
        {
            using var db = CreateInMemoryContext();

            var item = new PriceItems
            {
                BasePrice = 100,
                Unit = "ks",
                Task = "Test práce",
                Specification = "Specifikace",
                Material = "Materiál",
                Location = "Stěna",
                MaterialCoef = 1.2,
                PositionCoef = 1.0
            };

            db.PriceItems.Add(item);
            db.SaveChanges();

            Assert.That(db.PriceItems.Count(), Is.EqualTo(1));
        }

        // ============================================================
        // TEST 2 – SELECT
        // ============================================================

        /// <summary>
        /// Ověří, že lze načíst vložené položky.
        /// </summary>
        [Test]
        public void GetAll_Should_Return_Inserted_Items()
        {
            using var db = CreateInMemoryContext();

            db.PriceItems.Add(new PriceItems { Task = "A", Specification = "X" });
            db.PriceItems.Add(new PriceItems { Task = "B", Specification = "Y" });
            db.SaveChanges();

            var items = db.PriceItems.ToList();

            Assert.That(items.Count, Is.EqualTo(2));
        }

        // ============================================================
        // TEST 3 – UPDATE
        // ============================================================

        /// <summary>
        /// Ověří, že lze aktualizovat existující položku.
        /// </summary>
        [Test]
        public void Update_Should_Modify_Existing_Item()
        {
            using var db = CreateInMemoryContext();

            var item = new PriceItems { Task = "Původní" };
            db.PriceItems.Add(item);
            db.SaveChanges();

            item.Task = "Změněno";
            db.SaveChanges();

            var updated = db.PriceItems.First();

            Assert.That(updated.Task, Is.EqualTo("Změněno"));
        }

        // ============================================================
        // TEST 4 – DELETE
        // ============================================================

        /// <summary>
        /// Ověří, že lze smazat položku.
        /// </summary>
        [Test]
        public void Delete_Should_Remove_Item()
        {
            using var db = CreateInMemoryContext();

            var item = new PriceItems { Task = "Smazat" };
            db.PriceItems.Add(item);
            db.SaveChanges();

            db.PriceItems.Remove(item);
            db.SaveChanges();

            Assert.That(db.PriceItems.Count(), Is.EqualTo(0));
        }

        // ============================================================
        // TEST 5 – FULLNAME
        // ============================================================

        /// <summary>
        /// Ověří, že FullName vrací správný formát.
        /// </summary>
        [Test]
        public void FullName_Should_Combine_Fields()
        {
            var item = new PriceItems
            {
                Task = "Drážkování",
                Specification = "El. krabice",
                Material = "Beton",
                Location = "Stěna"
            };

            Assert.That(item.FullName, Is.EqualTo("Drážkování | El. krabice | Beton | Stěna"));
        }
    }
}
