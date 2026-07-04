using NUnit.Framework;

// ✔ Nutné pro AppDbContext
using ElektroOffer_app.Data;

// ✔ Nutné pro Material model
using ElektroOffer_app.Models;

// ✔ Nutné pro SQLite InMemory databázi
using Microsoft.Data.Sqlite;

// ✔ Nutné pro EF Core
using Microsoft.EntityFrameworkCore;

namespace ElektroOffer_app.Tests.Unit.RepositoryTests
{
    /// ==================================================================
    /// 🗄️ UNIT TESTS — Repository edge cases
    /// ==================================================================
    /// Tyto testy ověřují chování EF Core v extrémních situacích:
    ///   • vložení null objektu
    ///   • update neexistujícího záznamu
    ///   • delete neexistujícího záznamu
    ///
    /// DŮLEŽITÉ:
    /// Tvůj AppDbContext používá SQLite → testy musí používat SQLite InMemory.
    /// ==================================================================
    public class RepositoryEdgeCaseTests
    {
        private SqliteConnection? _connection;
        private AppDbContext? _db;

        // ------------------------------------------------------------------
        // 🔧 SETUP — vytvoření SQLite InMemory databáze
        // ------------------------------------------------------------------
        [SetUp]
        public void Setup()
        {
            // SQLite InMemory DB musí být otevřená po celou dobu testu
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)   // ✔ správně
                .Options;

            _db = new AppDbContext(options);
            _db.Database.EnsureCreated();
        }

        // ------------------------------------------------------------------
        // 🧹 CLEANUP — zavření DB
        // ------------------------------------------------------------------
        [TearDown]
        public void Cleanup()
        {
            _connection?.Close();
        }

        // ------------------------------------------------------------------
        // 🧪 TEST 1: Insert null objektu → EF Core vyhodí NullReferenceException
        // ------------------------------------------------------------------
        [Test]
        public void Insert_Should_Throw_When_Null()
        {
            // EF Core NEvyhazuje ArgumentNullException.
            // EF Core se pokusí vytvořit EntityEntry z null → NullReferenceException.
            Assert.Throws<NullReferenceException>(() =>
            {
                _db!.Materials.Add(null!);   // null! = vědomě předáváme null
                _db.SaveChanges();
            });
        }

        // ------------------------------------------------------------------
        // 🧪 TEST 2: Update neexistujícího záznamu → DbUpdateConcurrencyException
        // ------------------------------------------------------------------
        [Test]
        public void Update_Should_Throw_When_Item_Not_Found()
        {
            var material = new Material
            {
                Id = 999,
                Name = "X"
            };

            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                _db!.Materials.Update(material);
                _db.SaveChanges();
            });
        }

        // ------------------------------------------------------------------
        // 🧪 TEST 3: Delete neexistujícího záznamu → EF Core vyhodí DbUpdateConcurrencyException
        // ------------------------------------------------------------------
        [Test]
        public void Delete_Should_Throw_When_Item_Not_Found()
        {
            var material = new Material
            {
                Id = 999
            };

            // EF Core očekává, že DELETE ovlivní 1 řádek.
            // Pokud ovlivní 0 → DbUpdateConcurrencyException.
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                _db!.Materials.Remove(material);
                _db.SaveChanges();
            });
        }
    }
}

