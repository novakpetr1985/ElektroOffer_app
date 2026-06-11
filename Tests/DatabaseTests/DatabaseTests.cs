using NUnit.Framework;
using System;
using System.IO;
using System.Data.SQLite;
using System.Collections.Generic;

namespace ElektroOffer_app.tests.DatabaseTests
{
    [TestFixture]
    public class DatabaseTests
    {
        private string _dbPath;

        // =========================
        // SETUP
        // =========================
        [SetUp]
        public void Setup()
        {
            _dbPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "elektrooffer.db"
            );
        }

        // =========================
        // 1. SMOKE TEST – SOUBOR
        // =========================
        [Test]
        public void DB_File_Should_Exist()
        {
            Assert.IsTrue(File.Exists(_dbPath),
                $"Databáze neexistuje: {_dbPath}");
        }

        // =========================
        // 2. SMOKE TEST – PŘIPOJENÍ
        // =========================
        [Test]
        public void DB_Should_Be_Connectable()
        {
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();

            Assert.AreEqual(System.Data.ConnectionState.Open, conn.State);
        }

        // =========================
        // 3. STRUKTURA DB – TABULKY
        // =========================
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
            {
                tables.Add(reader.GetString(0));
            }

            Assert.Contains("Tasks", tables);
            Assert.Contains("Materials", tables);
            Assert.Contains("BaseMaterials", tables);
            Assert.Contains("Positions", tables);
            Assert.Contains("Specifications", tables);
            Assert.Contains("PriceItems", tables);
            Assert.Contains("PriceCatalog", tables);
        }

        // =========================
        // 4. DATA CHECK – MINIMÁLNÍ OBSAH
        // =========================
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