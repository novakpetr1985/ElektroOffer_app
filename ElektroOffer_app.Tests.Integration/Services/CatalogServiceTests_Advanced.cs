using NUnit.Framework;
using ElektroOffer_app.Services;
using ElektroOffer_app.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ElektroOffer_app.Models;


namespace ElektroOffer_app.Tests.Integration.Services
{
    /// =====================================================================
    /// 📦 INTEGRATION TESTS — CatalogService
    /// Testujeme reálné načítání ceníku z SQLite InMemory DB.
    /// =====================================================================
    public class CatalogServiceTests_Advanced
    {
        private SqliteConnection? _connection;
        private AppDbContext? _db;
        private CatalogService? _service;

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

        [TearDown]
        public void Cleanup()
        {
            _connection?.Close();
        }

        [Test]
        public void IsCatalogEmpty_Should_Return_True_When_No_Data()
        {
            var result = _service!.IsCatalogEmpty(_db!);
            Assert.IsTrue(result);
        }

        [Test]
        public void LoadCatalog_Should_Return_Data_When_Db_Contains_Items()
        {
            _db!.PriceItems.Add(new PriceItems
            {
                Task = "Montáž kabelu",
                Specification = "CYKY 3x2,5",
                Material = "CYKY",
                Location = "Stěna",
                Unit = "m",
                BasePrice = 50,
                MaterialCoef = 1.1,
                PositionCoef = 1.0
            });

            _db.Materials.Add(new Material
            {
                Name = "CYKY 3x2,5",
                Price = 20
            });

            _db.SaveChanges();

            var (tasks, materials) = _service!.LoadCatalog(_db!);

            Assert.IsNotEmpty(tasks);
            Assert.IsTrue(tasks.Contains("Montáž kabelu"));

            Assert.IsNotEmpty(materials);
            Assert.IsTrue(materials.Any(m => m.Name == "CYKY 3x2,5"));
        }
    }
}
