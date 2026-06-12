using NUnit.Framework;
using Microsoft.Data.Sqlite;

namespace ElektroOffer_app.Tests.Integration.Database;

[TestFixture]
public class DatabaseSchemaTests
{
    // =========================================================
    // 🔧 KONFIGURACE — připojení k in-memory SQLite databázi
    // =========================================================

    private string _connectionString;

    [SetUp]
    public void Setup()
    {
        // SQLite databáze uložená pouze v paměti
        _connectionString = "Data Source=:memory:;Cache=Shared";
    }

    // =========================================================
    // 🧪 TEST — vytvoření tabulky a ověření, že existuje
    // =========================================================

    /// <summary>
    /// Ověří, že lze vytvořit tabulku Products a že je následně
    /// viditelná v SQLite systémové tabulce sqlite_master.
    /// </summary>
    [Test]
    public void Should_Create_Products_Table()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // SQL příkaz pro vytvoření tabulky
        var createTableSql = @"
            CREATE TABLE IF NOT EXISTS Products (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL
            );
        ";

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = createTableSql;
            cmd.ExecuteNonQuery();
        }

        // Ověření, že tabulka existuje v sqlite_master
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Products';";
            var result = cmd.ExecuteScalar();

            Assert.That(result, Is.EqualTo("Products"), "Tabulka Products nebyla vytvořena.");
        }
    }
}
