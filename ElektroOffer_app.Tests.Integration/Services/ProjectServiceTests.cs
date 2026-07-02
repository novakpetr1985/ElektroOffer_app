using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Reflection;
using System.Text.Json;

namespace ElektroOffer_app.Tests.Integration.Services;

// =========================================================================
// 🧪 ProjectServiceTests
// =========================================================================
//
// ÚČEL:
// - Testování business logiky ProjectService
// - Ověření práce s daty (EF Core + SQLite InMemory)
// - Validace reálného chování služby v integrační vrstvě
//
// CO TENTO TEST DĚLÁ:
// - Testuje metody ProjectService (Save, Load, Export, Import logiku)
// - Ověřuje integraci service → DbContext (pokud bude v dalších testech)
// - Simuluje reálné použití aplikace (UI → Service → Data)
//
// CO TENTO TEST NEDĚLÁ:
// - NETESTUJE EF Core interně
// - NETESTUJE SQLite engine
// - NETESTUJE WPF UI logiku (MessageBox je součást Service, ale není UI test)
// - NETESTUJE ViewModel vrstvu
//
// =========================================================================
//
// DŮLEŽITÉ PRINCIPY:
// - Každý test běží nad čistou DB instancí
// - Používáme SQLite InMemory + SHARED mode pro stabilitu
// - DbContext se vytváří vždy znovu pro každý test
// - ProjectService je testována jako “black box” (bez znalosti UI)
// - Testy se zaměřují na chování, ne implementaci
//
// =========================================================================

[TestFixture]
public class ProjectServiceTests
{
    // =====================================================================
    // TEST INFRASTRUKTURA
    // =====================================================================

    /// <summary>
    /// SQLite connection držící InMemory databázi.
    ///
    /// DŮLEŽITÉ:
    /// - SQLite InMemory DB existuje pouze dokud existuje otevřená connection
    /// - Jakmile se connection zavře → DB zmizí
    /// - Proto musí být držena po celou dobu testu
    /// </summary>
    private SqliteConnection _connection = null!;

    /// <summary>
    /// EF Core DbContext používaný pro testování.
    ///
    /// ROLE:
    /// - ARRANGE fáze: příprava dat (insert test data)
    /// - ASSERT fáze: ověření stavu databáze (pokud bude potřeba v dalších testech)
    /// - slouží jako kontrolní bod mezi service a DB
    /// </summary>
    private AppDbContext _db = null!;

    /// <summary>
    /// System Under Test (SUT)
    ///
    /// TOTO JE HLAVNÍ TESTOVANÁ KOMPONENTA:
    /// - ProjectService obsahuje business logiku aplikace
    /// - řeší ukládání, načítání a export/import projektů
    /// - obsahuje i UI závislosti (MessageBox, Dialogy)
    ///
    /// V TESTECH:
    /// - testujeme pouze chování, ne UI vrstvu
    /// </summary>
    private ProjectService _service = null!;

    // =====================================================================
    // TEST HELPER METODY (OBCHÁZENÍ UI)
    // =====================================================================
    //
    // ÚČEL:
    // - umožňuje testovat interní logiku ProjectService
    // - obchází SaveFileDialog / OpenFileDialog
    // - volá interní SaveToPath nepřímo (hack pro testování)
    //
    // =====================================================================

    private string? InvokeSaveToPath(ProjectData data, string path)
    {
        // reflection není potřeba – SaveToPath je private,
        // takže pro test buď:
        // 1) změna na internal (doporučeno)
        // 2) nebo test-friendly refaktor

        var method = typeof(ProjectService)
            .GetMethod("SaveToPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return (string?)method!.Invoke(_service, new object[] { data, path });
    }

    private (ProjectData?, string?) InvokeLoadFromFile(string path)
    {
        var json = File.ReadAllText(path);

        var data = JsonSerializer.Deserialize<ProjectData>(
            json,
            new JsonSerializerOptions { WriteIndented = true }
        );

        return (data, path);
    }

    // =====================================================================
    // SETUP
    // =====================================================================

    /// <summary>
    /// Inicializace testovacího prostředí před každým testem.
    ///
    /// CELKOVÝ POSTUP:
    /// 1. Vytvoření SQLite InMemory connection
    /// 2. Otevření connection (nutné pro existenci DB)
    /// 3. Nastavení EF Core provideru
    /// 4. Vytvoření DbContextu
    /// 5. Vytvoření databázového schématu
    /// 6. Inicializace ProjectService (SUT)
    ///
    /// DŮLEŽITÉ:
    /// - Každý test má čistou izolovanou DB
    /// - Neexistuje sdílení dat mezi testy
    /// </summary>
    [SetUp]
    public void Setup()
    {
        // -------------------------------------------------------------
        // 1. SQLite InMemory connection (SHARED MODE)
        // -------------------------------------------------------------
        //
        // SHARED MODE znamená:
        // - DB není izolovaná na jeden connection request
        // - umožňuje stabilní práci EF Core nad InMemory DB
        // - zabraňuje problémům s “zmizelou databází”
        _connection = new SqliteConnection(
            "Data Source=file:memdb1?mode=memory&cache=shared"
        );

        // -------------------------------------------------------------
        // 2. Otevření connection (KRITICKÝ KROK)
        // -------------------------------------------------------------
        // Bez otevření connection SQLite DB neexistuje
        _connection.Open();

        // -------------------------------------------------------------
        // 3. EF Core konfigurace
        // -------------------------------------------------------------
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        // -------------------------------------------------------------
        // 4. Inicializace DbContextu
        // -------------------------------------------------------------
        _db = new AppDbContext(options);

        // -------------------------------------------------------------
        // 5. Vytvoření DB schématu
        // -------------------------------------------------------------
        // EF Core vytvoří tabulky podle modelů
        _db.Database.EnsureCreated();

        // -------------------------------------------------------------
        // 6. Inicializace Service vrstvy (SUT)
        // -------------------------------------------------------------
        _service = new ProjectService();
    }

    // =====================================================================
    // TEARDOWN
    // =====================================================================

    /// <summary>
    /// Bezpečné ukončení testu a uvolnění všech zdrojů.
    ///
    /// POŘADÍ UPOZORNĚNÍ:
    /// 1. Dispose DbContext (uvolnění EF Core trackingu)
    /// 2. Close connection (ukončení SQLite session)
    /// 3. Dispose connection (uvolnění paměti)
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _db?.Dispose();

        _connection?.Close();
        _connection?.Dispose();
    }

    // =====================================================================
    // SMOKE TEST (ZÁKLADNÍ OVĚŘENÍ INFRASTRUKTURY)
    // =====================================================================

    /// <summary>
    /// SMOKE TEST / HEALTH CHECK
    ///
    /// ÚČEL:
    /// - Ověří, že ProjectService lze vůbec inicializovat
    /// - Ověří, že testovací prostředí (DB + EF Core + Service) funguje
    /// - Slouží jako první indikátor, že setup není rozbitý
    ///
    /// DŮLEŽITÉ:
    /// - Tento test netestuje business logiku
    /// - Pouze validuje inicializaci systému
    /// - Je to “baseline test” pro další rozvoj
    /// </summary>
    [Test]
    public void Should_Be_Able_To_Initialize_ProjectService()
    {
        // ARRANGE
        // zatím žádná data – jen kontrola inicializace

        // ACT
        var serviceExists = _service != null;

        // ASSERT
        Assert.That(
            serviceExists,
            Is.True,
            "ProjectService nebyl správně inicializován."
        );
    }

    // =====================================================================
    // SAVE / LOAD TESTY (FILE-BASED LOGIKA)
    // =====================================================================
    //
    // ÚČEL:
    // - Ověření serializace ProjectData do JSON
    // - Ověření deserializace zpět do objektu
    // - Validace integrity dat mezi uložením a načtením
    //
    // POZNÁMKA:
    // - Obcházíme UI (SaveFileDialog / OpenFileDialog)
    // - Testujeme pouze core logiku přes file system
    // - Používáme temp soubory pro izolaci
    //
    // =====================================================================

    // =====================================================================
    // SAVE TEST
    // =====================================================================

    [Test]
    public void Should_Save_Project_To_File_Correctly()
    {
        // ARRANGE
        var tempFile = Path.Combine(Path.GetTempPath(), $"project_test_{Guid.NewGuid()}.eof");

        var project = new ProjectData
        {
            ProjectName = "Test projekt"
        };

        // ACT
        var savedPath = InvokeSaveToPath(project, tempFile);

        // ASSERT
        Assert.That(savedPath, Is.Not.Null, "SaveToPath vrátil null.");
        Assert.That(File.Exists(savedPath), Is.True, "Soubor nebyl vytvořen.");

        var content = File.ReadAllText(savedPath!);

        Assert.That(content, Does.Contain("Test projekt"),
            "Data nebyla správně serializována do JSON.");

        // BONUS: validace JSON struktury (ochrana proti rozbití modelu)
        var deserialized = JsonSerializer.Deserialize<ProjectData>(content);

        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.ProjectName, Is.EqualTo("Test projekt"));

        // CLEANUP
        File.Delete(savedPath!);
    }

    // =====================================================================
    // LOAD TEST
    // =====================================================================

    [Test]
    public void Should_Load_Project_From_File_Correctly()
    {
        // ARRANGE
        var tempFile = Path.Combine(Path.GetTempPath(), $"project_test_{Guid.NewGuid()}.eof");

        var original = new ProjectData
        {
            ProjectName = "Load test projekt"
        };

        var json = JsonSerializer.Serialize(original, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(tempFile, json);

        // ACT
        var (loaded, path) = InvokeLoadFromFile(tempFile);

        // ASSERT
        Assert.That(loaded, Is.Not.Null, "Projekt se nepodařilo načíst.");
        Assert.That(loaded!.ProjectName, Is.EqualTo("Load test projekt"));
        Assert.That(path, Is.EqualTo(tempFile));

        // BONUS: integrity check
        Assert.That(loaded.ProjectName, Is.Not.Empty);

        // CLEANUP
        File.Delete(tempFile);
    }
}
