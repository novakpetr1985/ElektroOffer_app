using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;  // ← ZMĚNĚNO: z System.Data.SQLite

namespace ElektroOffer_app.Tests.Unit.DatabaseTests
{
    /// <summary>
    /// Integration testy SQLite databáze.
    /// Každý test dostane vlastní izolovanou DB v temp složce.
    /// 
    /// POZOR: Používáme Microsoft.Data.Sqlite (nový balíček kompatibilní s EF Core),
    /// nikoliv System.Data.SQLite (starý balíček – odebrán z projektu).
    /// Rozdíly oproti System.Data.SQLite:
    ///   - žádné SQLiteConnection.CreateFile() → soubor vznikne automaticky při Open()
    ///   - žádné ClearAllPools() → Microsoft.Data.Sqlite nepotřebuje ruční uvolnění poolů
    ///   - connection string: "Data Source=cesta.db" (bez "Version=3;")
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
            // Vytvoří unikátní DB pro každý test
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
        /// Microsoft.Data.Sqlite uvolňuje file-lock při Dispose() automaticky,
        /// ale GC.Collect() přidáváme jako pojistku pro případ že někde
        /// zůstal neuvolněný SqliteConnection objekt.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            // ============================================
            // 1) Donutit GC dokončit finalizaci
            //    - pojistka pro případ neuvolněného connection objektu
            //    - Microsoft.Data.Sqlite nepotřebuje ClearAllPools()
            // ============================================
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // ============================================
            // 2) Smazání testovací databáze
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
        /// 
        /// Microsoft.Data.Sqlite vytvoří soubor automaticky při Open(),
        /// pokud ještě neexistuje – není potřeba CreateFile().
        /// </summary>
        private void CreateTestDatabase(string path)
        {
            // ← ZMĚNĚNO: žádné CreateFile() – soubor vznikne při conn.Open()
            // Connection string bez "Version=3;" – Microsoft.Data.Sqlite ho nepodporuje
            using var conn = new SqliteConnection($"Data Source={path}");
            conn.Open();

            using var cmd = conn.CreateCommand();

            // Vytvoření tabulek
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

            // Minimální testovací data
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
            // ← ZMĚNĚNO: SqliteConnection (Microsoft) místo SQLiteConnection (System.Data)
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
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
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
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
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
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
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM PriceCatalog;";

            var count = Convert.ToInt32(cmd.ExecuteScalar());

            Assert.Greater(count, 0, "PriceCatalog je prázdný");
        }
    }
}