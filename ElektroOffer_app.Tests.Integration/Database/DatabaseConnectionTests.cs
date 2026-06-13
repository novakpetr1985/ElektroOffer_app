using NUnit.Framework;
using Microsoft.Data.Sqlite;

namespace ElektroOffer_app.Tests.Integration.Database;

[TestFixture]
public class DatabaseConnectionTests
{
    // =========================================================
    // 🔧 KONFIGURACE TESTŮ — příprava připojení k SQLite
    // =========================================================

    /// <summary>
    /// Connection string pro dočasnou SQLite databázi v paměti.
    /// </summary>
    private string _connectionString = string.Empty;

    // =========================================================
    // 🧪 SETUP — spouští se před KAŽDÝM testem
    // =========================================================

    /// <summary>
    /// Inicializace před každým testem.
    /// Vytvoříme čistou in-memory databázi.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        // SQLite databáze uložená pouze v paměti
        // Cache=Shared umožňuje sdílet připojení mezi více connection objekty
        _connectionString = "Data Source=:memory:;Cache=Shared";
    }

    // =========================================================
    // 🧪 TEST 1 — Ověření, že se lze připojit k databázi
    // =========================================================

    /// <summary>
    /// Ověří, že SQLite databáze lze úspěšně otevřít.
    /// Toto je základní integrační test — pokud selže,
    /// nemá smysl pokračovat v dalších DB testech.
    /// </summary>
    [Test]
    public void Should_Connect_To_Database()
    {
        // Vytvoříme nové připojení
        using var connection = new SqliteConnection(_connectionString);

        // Pokusíme se otevřít databázi
        connection.Open();

        // Ověříme, že připojení je skutečně otevřené
        Assert.That(
            connection.State,
            Is.EqualTo(System.Data.ConnectionState.Open),
            "SQLite databáze se nepodařilo otevřít."
        );
    }
}
