
using NUnit.Framework;
using ElektroOffer_app.Services;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace ElektroOffer_app.Tests.Unit.Services
{
    /// =====================================================================
    /// 🧪 UNIT TESTS — CatalogService
    /// =====================================================================
    /// Tyto testy ověřují:
    ///   • načítání ceníku práce (Tasks)
    ///   • načítání ceníku materiálu (Materials)
    ///   • správné chování Distinct() u Tasks
    ///   • správné chování IsCatalogEmpty()
    ///   • práci s SQLite InMemory databází
    ///
    /// CatalogService je čistá logika bez UI → ideální pro unit testy.
    /// =====================================================================
    public class CatalogServiceTests
    {
        private SqliteConnection? _connection;
        private AppDbContext? _db;
        private CatalogService? _service;

        // ------------------------------------------------------------------
        // 🧰 SETUP — vytvoření izolované SQLite InMemory DB
        // ------------------------------------------------------------------
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

        // ------------------------------------------------------------------
        // 🧹 TEARDOWN — uzavření DB
        // ------------------------------------------------------------------
        [TearDown]
        public void Cleanup()
        {
            _connection?.Close();
        }

        // ------------------------------------------------------------------
        // 🧪 TEST 1: LoadCatalog načte unikátní Tasks
        // ------------------------------------------------------------------
        [Test]
        public void LoadCatalog_Should_Return_Distinct_Tasks()
        {
            // Arrange
            _db!.PriceItems.AddRange(new[]
            {
                new PriceItems { Task = "Montáž" },
                new PriceItems { Task = "Montáž" }, // duplicita
                new PriceItems { Task = "Demontáž" }
            });
            _db.SaveChanges();

            // Act
            var (tasks, _) = _service!.LoadCatalog(_db);

            // Assert
            Assert.AreEqual(2, tasks.Count);
            Assert.Contains("Montáž", tasks);
            Assert.Contains("Demontáž", tasks);
        }

        // ------------------------------------------------------------------
        // 🧪 TEST 2: LoadCatalog načte všechny materiály
        // ------------------------------------------------------------------
        [Test]
        public void LoadCatalog_Should_Return_All_Materials()
        {
            // Arrange
            _db!.Materials.AddRange(new[]
            {
                new Material { Name = "CYKY", Price = 20 },
                new Material { Name = "Lanko", Price = 15 }
            });
            _db.SaveChanges();

            // Act
            var (_, materials) = _service!.LoadCatalog(_db);

            // Assert
            Assert.AreEqual(2, materials.Count);
            Assert.AreEqual("CYKY", materials[0].Name);
            Assert.AreEqual("Lanko", materials[1].Name);
        }

        // ------------------------------------------------------------------
        // 🧪 TEST 3: LoadCatalog vrací prázdné seznamy, pokud DB je prázdná
        // ------------------------------------------------------------------
        [Test]
        public void LoadCatalog_Should_Return_Empty_Lists_When_Database_Is_Empty()
        {
            // Act
            var (tasks, materials) = _service!.LoadCatalog(_db!);

            // Assert
            Assert.IsEmpty(tasks);
            Assert.IsEmpty(materials);
        }

        // ------------------------------------------------------------------
        // 🧪 TEST 4: IsCatalogEmpty vrací true, pokud DB je prázdná
        // ------------------------------------------------------------------
        [Test]
        public void IsCatalogEmpty_Should_Return_True_When_No_Data()
        {
            Assert.IsTrue(_service!.IsCatalogEmpty(_db!));
        }

        // ------------------------------------------------------------------
        // 🧪 TEST 5: IsCatalogEmpty vrací false, pokud existují PriceItems
        // ------------------------------------------------------------------
        [Test]
        public void IsCatalogEmpty_Should_Return_False_When_PriceItems_Exist()
        {
            _db!.PriceItems.Add(new PriceItems { Task = "Montáž" });
            _db.SaveChanges();

            Assert.IsFalse(_service!.IsCatalogEmpty(_db));
        }

        // ------------------------------------------------------------------
        // 🧪 TEST 6: IsCatalogEmpty vrací false, pokud existují Materials
        // ------------------------------------------------------------------
        [Test]
        public void IsCatalogEmpty_Should_Return_False_When_Materials_Exist()
        {
            _db!.Materials.Add(new Material { Name = "CYKY", Price = 20 });
            _db.SaveChanges();

            Assert.IsFalse(_service!.IsCatalogEmpty(_db));
        }

        // ------------------------------------------------------------------
        // 🧪 TEST 7: LoadCatalog — kombinace PriceItems + Materials
        // ------------------------------------------------------------------
        [Test]
        public void LoadCatalog_Should_Return_Correct_Data_When_Both_Tables_Have_Data()
        {
            _db!.PriceItems.Add(new PriceItems { Task = "Montáž" });
            _db.Materials.Add(new Material { Name = "CYKY", Price = 20 });
            _db.SaveChanges();

            var (tasks, materials) = _service!.LoadCatalog(_db);

            Assert.AreEqual(1, tasks.Count);
            Assert.AreEqual(1, materials.Count);
        }
    }
}
