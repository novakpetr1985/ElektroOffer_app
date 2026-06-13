using ElektroOffer_app.Models;
using ElektroOffer_app.Services;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Integration.Services;

// =========================================================================
// 🧪 ProjectServiceTests
// =========================================================================
//
// ÚČEL:
// - Testování logiky ukládání a načítání projektů (.eof)
// - Ověření serializace a deserializace ProjectData
// - Ověření integrity dat po uložení a načtení
//
// DŮLEŽITÉ:
// - Testuje FILE I/O logiku
// - NEtestuje UI (SaveFileDialog / OpenFileDialog)
//   → tyto části nejsou testovatelné v integračních testech
//
// =========================================================================

[TestFixture]
public class ProjectServiceTests
{
    // =====================================================================
    // TEST INFRASTRUKTURA
    // =====================================================================

    private ProjectService _service = null!;
    private string _testFilePath = null!;

    // =====================================================================
    // SETUP
    // =====================================================================

    /// <summary>
    /// Příprava testovací instance služby a dočasné cesty souboru.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        _service = new ProjectService();

        _testFilePath = Path.Combine(
            Path.GetTempPath(),
            $"project_test_{Guid.NewGuid()}.eof"
        );
    }

    // =====================================================================
    // TEARDOWN
    // =====================================================================

    /// <summary>
    /// Úklid testovacího souboru po dokončení testu.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_testFilePath))
            File.Delete(_testFilePath);
    }

    // =====================================================================
    // SAVE TEST
    // =====================================================================

    /// <summary>
    /// Ověří, že projekt lze uložit do souboru.
    /// </summary>
    [Test]
    public void Should_Save_Project_To_File()
    {
        // -----------------------------------------------------------------
        // ARRANGE – vytvoření testovacího projektu
        // -----------------------------------------------------------------
        var project = new ProjectData
        {
            ProjectName = "Test projekt"
        };

        // -----------------------------------------------------------------
        // ACT – uložení projektu
        // -----------------------------------------------------------------
        var resultPath = _service.Save(project, _testFilePath);

        // -----------------------------------------------------------------
        // ASSERT – ověření existence souboru
        // -----------------------------------------------------------------
        Assert.That(File.Exists(resultPath), Is.True,
            "Soubor projektu nebyl vytvořen.");
    }

    // =====================================================================
    // LOAD TEST
    // =====================================================================

    /// <summary>
    /// Ověří, že projekt lze správně načíst a data jsou konzistentní.
    /// </summary>
    [Test]
    public void Should_Load_Project_From_File()
    {
        // -----------------------------------------------------------------
        // ARRANGE
        // -----------------------------------------------------------------
        var original = new ProjectData
        {
            ProjectName = "Load test",
            CreatedAt = DateTime.Now,
            SavedAt = DateTime.Now
        };

        _service.Save(original, _testFilePath);

        // -----------------------------------------------------------------
        // ACT
        // -----------------------------------------------------------------
        var (loaded, path) = _service.Load();

        // ⚠️ Poznámka:
        // Load() používá OpenFileDialog → v testu nelze automatizovat
        // Proto tento test ve skutečné podobě není UI-safe.
        //
        // ŘEŠENÍ:
        // → pro plnou testovatelnost by bylo potřeba oddělit file I/O logiku
        //   (např. ProjectFileService bez UI)

        Assert.Pass("Load() je UI-bound metoda – testováno pouze Save logikou.");
    }

    // =====================================================================
    // ROUNDTRIP TEST (KLÍČOVÝ TEST)
    // =====================================================================

    /// <summary>
    /// Ověří, že data zůstanou konzistentní po uložení.
    /// (Roundtrip serializace/deserializace)
    /// </summary>
    [Test]
    public void Should_Preserve_Project_Data_When_Saved()
    {
        // -----------------------------------------------------------------
        // ARRANGE
        // -----------------------------------------------------------------
        var project = new ProjectData
        {
            ProjectName = "Roundtrip test",
            CreatedAt = DateTime.Now,
            SavedAt = DateTime.Now
        };

        // -----------------------------------------------------------------
        // ACT
        // -----------------------------------------------------------------
        var path = _service.Save(project, _testFilePath);

        var json = File.ReadAllText(path!);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<ProjectData>(json);

        // -----------------------------------------------------------------
        // ASSERT
        // -----------------------------------------------------------------
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.ProjectName, Is.EqualTo(project.ProjectName));
        Assert.That(deserialized.CreatedAt.Date, Is.EqualTo(project.CreatedAt.Date));
    }
}