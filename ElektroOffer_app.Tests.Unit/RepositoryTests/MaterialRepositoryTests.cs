using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Linq;

namespace ElektroOffer_app.Tests.Unit.RepositoryTests
{
    /// <summary>
    /// Testy EF Core DbContextu pro tabulku Materials.
    /// Používá SQLite InMemory databázi, která se chová stejně jako reálná SQLite,
    /// ale je rychlá, izolovaná a ideální pro unit/integration testy.
    /// </summary>
    [TestFixture]
    public class MaterialRepositoryTests
    {
        /// <summary>
        /// Vytvoří nový EF Core kontext s InMemory SQLite databází.
        /// Každý test dostane vlastní izolovanou databázi.
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

            // vytvoření tabulek podle modelů
            context.Database.EnsureCreated();

            return context;
        }

        // ============================================================
        // TEST 1 – INSERT
        // ============================================================

        /// <summary>
        /// Ověří, že lze vložit nový materiál.
        /// </summary>
        [Test]
        public void Insert_Should_Add_New_Material()
        {
            using var db = CreateInMemoryContext();

            var material = new Material
            {
                Name = "Kabel CYKY 3x1,5",
                Price = 12.5,
                Unit = "m"
            };

            db.Materials.Add(material);
            db.SaveChanges();

            Assert.That(db.Materials.Count(), Is.EqualTo(1));
        }

        // ============================================================
        // TEST 2 – SELECT
        // ============================================================

        /// <summary>
        /// Ověří, že lze načíst vložené materiály.
        /// </summary>
        [Test]
        public void GetAll_Should_Return_Inserted_Materials()
        {
            using var db = CreateInMemoryContext();

            db.Materials.Add(new Material { Name = "A", Price = 10, Unit = "ks" });
            db.Materials.Add(new Material { Name = "B", Price = 20, Unit = "m" });
            db.SaveChanges();

            var items = db.Materials.ToList();

            Assert.That(items.Count, Is.EqualTo(2));
        }

        // ============================================================
        // TEST 3 – UPDATE
        // ============================================================

        /// <summary>
        /// Ověří, že lze aktualizovat existující materiál.
        /// </summary>
        [Test]
        public void Update_Should_Modify_Existing_Material()
        {
            using var db = CreateInMemoryContext();

            var material = new Material { Name = "Původní", Price = 10, Unit = "ks" };
            db.Materials.Add(material);
            db.SaveChanges();

            material.Name = "Změněno";
            material.Price = 99.9;
            db.SaveChanges();

            var updated = db.Materials.First();

            Assert.That(updated.Name, Is.EqualTo("Změněno"));
            Assert.That(updated.Price, Is.EqualTo(99.9));
        }

        // ============================================================
        // TEST 4 – DELETE
        // ============================================================

        /// <summary>
        /// Ověří, že lze smazat materiál.
        /// </summary>
        [Test]
        public void Delete_Should_Remove_Material()
        {
            using var db = CreateInMemoryContext();

            var material = new Material { Name = "Smazat", Price = 5, Unit = "ks" };
            db.Materials.Add(material);
            db.SaveChanges();

            db.Materials.Remove(material);
            db.SaveChanges();

            Assert.That(db.Materials.Count(), Is.EqualTo(0));
        }

        // ============================================================
        // TEST 5 – VALIDACE DAT
        // ============================================================

        /// <summary>
        /// Ověří, že model Material má správné výchozí hodnoty.
        /// </summary>
        [Test]
        public void Material_Default_Values_Should_Be_Valid()
        {
            var material = new Material();

            Assert.That(material.Name, Is.EqualTo(string.Empty));
            Assert.That(material.Unit, Is.EqualTo(string.Empty));
            Assert.That(material.Price, Is.EqualTo(0));
        }
    }
}
