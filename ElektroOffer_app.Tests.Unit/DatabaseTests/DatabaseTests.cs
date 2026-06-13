using NUnit.Framework;
using System;
using System.IO;
using System.Data.SQLite;
using System.Collections.Generic;

namespace ElektroOffer_app.Tests.Unit.DatabaseTests
{
    /// <summary>
    /// Integration testy SQLite databáze.
    /// Každý test dostane vlastní izolovanou DB v temp složce.
    /// </summary>
    [TestFixture]
    public class DatabaseTests
    {
        /// <summary>
        /// Cesta k testovací databázi.
        /// Vytváří se v [SetUp] a maže v [TearDown].
        /// </summary>
        private string _dbPath = string.Empty;

        // ============================================================
        // SETUP
        // ============================================================

        /// <summary>
        /// Vytvoří novou testovací SQLite databázi před každým testem.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // vytvoří unikátní DB pro každý test
            _dbPath = Path.Combine(
                Path.GetTempPath(),
                $"elektrooffer_test_{Guid.NewGuid()}.db"
            );

            CreateTestDatabase(_dbPath);
        }

        // ============================================================
        // TEARDOWN
        // ============================================================

        /// <summary>
        /// Po testu smaže testovací databázi.
        /// SQLite někdy drží file-lock i po Dispose(),
        /// proto je nutné explicitně uvolnit connection pool
        /// a donutit GC dokončit finalizaci.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            // ============================================
            // 1) Uvolnění všech SQLite connection poolů
            //    - SQLite si drží file-lock i po Dispose()
            //    - ClearAllPools() okamžitě uvolní všechny handly
            // ============================================
            SQLiteConnection.ClearAllPools();

            // ============================================
            // 2) Donutit Garbage Collector dokončit finalizaci
            //    - některé SQLite objekty uvolňují lock až ve finalizéru
            //    - GC.Collect + WaitForPendingFinalizers zajistí,
            //      že file-lock bude opravdu uvolněn
            // ============================================
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // ============================================
            // 3) Smazání testovací databáze
            //    - nyní je soubor 100% volný
            // ============================================
            if (File.Exists(_dbPath))
                File.Delete(_dbPath);
        }

        // ============================================================
        // CREATE TEST DB
        // ============================================================

        /// <summary>
        /// Vytvoří strukturu databáze pro testy.
        /// Jednoduchý seed bez business logiky aplikace.
        /// </summary>
        private void CreateTestDatabase(string path)
        {
            // vytvoření prázdné DB
            SQLiteConnection.CreateFile(path);

            using var conn = new SQLiteConnection($"Data Source={path};Version=3;");
            conn.Open();

            using var cmd = conn.CreateCommand();

            // vytvoření tabulek
            cmd.CommandText = @"
                CREATE TABLE Tasks (Id INTEGER PRIMARY KEY, Name TEXT);
                CREATE TABLE Materials (Id INTEGER PRIMARY KEY);
                CREATE TABLE BaseMaterials (Id INTEGER PRIMARY KEY);
                CREATE TABLE Positions (Id INTEGER PRIMARY KEY);
                CREATE TABLE Specifications (Id INTEGER PRIMARY KEY);
                CREATE TABLE PriceItems (Id INTEGER PRIMARY KEY);
                CREATE TABLE PriceCatalog (Id INTEGER PRIMARY KEY);
            ";
            cmd.ExecuteNonQuery();

            // minimální test data
            cmd.CommandText = "INSERT INTO Tasks (Name) VALUES ('Test Task');";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO PriceCatalog (Id) VALUES (1);";
            cmd.ExecuteNonQuery();
        }

        // ============================================================
        // TEST 1 – FILE EXISTS
        // ============================================================

        /// <summary>
        /// Ověří, že test DB byla vytvořena na disku.
        /// </summary>
        [Test]
        public void DB_File_Should_Exist()
        {
            Assert.IsTrue(File.Exists(_dbPath),
                $"Test DB neexistuje: {_dbPath}");
        }

        // ============================================================
        // TEST 2 – CONNECTION
        // ============================================================

        /// <summary>
        /// Ověří, že SQLite DB lze otevřít.
        /// </summary>
        [Test]
        public void DB_Should_Be_Connectable()
        {
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();

            Assert.That(conn.State,
                Is.EqualTo(System.Data.ConnectionState.Open));
        }

        // ============================================================
        // TEST 3 – TABLES
        // ============================================================

        /// <summary>
        /// Ověří, že DB obsahuje všechny klíčové tabulky.
        /// </summary>
        [Test]
        public void DB_Should_Contain_Core_Tables()
        {
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT name 
                FROM sqlite_master 
                WHERE type='table';
            ";

            var tables = new List<string>();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                tables.Add(reader.GetString(0));

            Assert.Multiple(() =>
            {
                Assert.Contains("Tasks", tables);
                Assert.Contains("Materials", tables);
                Assert.Contains("BaseMaterials", tables);
                Assert.Contains("Positions", tables);
                Assert.Contains("Specifications", tables);
                Assert.Contains("PriceItems", tables);
                Assert.Contains("PriceCatalog", tables);
            });
        }

        // ============================================================
        // TEST 4 – DATA CHECK
        // ============================================================

        /// <summary>
        /// Ověří, že tabulka Tasks obsahuje data.
        /// </summary>
        [Test]
        public void Tasks_Should_Not_Be_Empty()
        {
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Tasks;";

            var count = Convert.ToInt32(cmd.ExecuteScalar());

            Assert.Greater(count, 0, "Tasks tabulka je prázdná");
        }

        /// <summary>
        /// Ověří, že PriceCatalog obsahuje data.
        /// </summary>
        [Test]
        public void PriceCatalog_Should_Not_Be_Empty()
        {
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM PriceCatalog;";

            var count = Convert.ToInt32(cmd.ExecuteScalar());

            Assert.Greater(count, 0, "PriceCatalog je prázdný");
        }
    }
}
