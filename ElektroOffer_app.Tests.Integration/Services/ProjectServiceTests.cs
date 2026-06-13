using ElektroOffer_app.Models;
using ElektroOffer_app.Services;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Integration.Services;

// =========================================================================
// 🧪 ProjectServiceTests
// =========================================================================
//
// ÚČEL:
// - Ověření práce s projekty (.eof soubory)
// - Testuje serializaci a deserializaci ProjectData
// - Ověřuje integritu dat při uložení a načtení
//
// DŮLEŽITÉ:
// - Testuje file I/O logiku služby
// - Nejde o UI ani databázi
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
    /// Příprava testovací služby a dočasné cesty.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        _service = new ProjectService();

        _testFilePath = Path.Combine(
            Path.GetTempPath(),
            $"test_project_{Guid.NewGuid()}.eof"
        );
    }

    // =====================================================================
    // TEARDOWN
    // =====================================================================

    /// <summary>
    /// Smazání testovacích souborů po testu.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_testFilePath))
            File.Delete(_testFilePath);
    }

    // =====================================================================
    // SAVE PROJECT
    // =====================================================================

    /// <summary>
    /// Ověří, že projekt lze uložit do souboru.
    /// </summary>
    [Test]
    public void Should_Save_Project_To_File()
    {
        // Arrange
        var project = new ProjectData
        {
            Name = "Test projekt",
            TotalPrice = 1000
        };

        // Act
        _service.SaveProject(_testFilePath, project);

        // Assert
        Assert.That(File.Exists(_testFilePath), Is.True);
    }

    // =====================================================================
    // LOAD PROJECT
    // =====================================================================

    /// <summary>
    /// Ověří, že projekt lze správně načíst.
    /// </summary>
    [Test]
    public void Should_Load_Project_From_File()
    {
        // Arrange
        var project = new ProjectData
        {
            Name = "Load test",
            TotalPrice = 2000
        };

        _service.SaveProject(_testFilePath, project);

        // Act
        var loaded = _service.LoadProject(_testFilePath);

        // Assert
        Assert.That(loaded.Name, Is.EqualTo("Load test"));
        Assert.That(loaded.TotalPrice, Is.EqualTo(2000));
    }

    // =====================================================================
    // ROUNDTRIP TEST
    // =====================================================================

    /// <summary>
    /// Ověří, že data zůstanou konzistentní
    /// po uložení a opětovném načtení.
    /// </summary>
    [Test]
    public void Should_Preserve_Data_Without_Loss()
    {
        // Arrange
        var project = new ProjectData
        {
            Name = "Roundtrip",
            TotalPrice = 9999
        };

        // Act
        _service.SaveProject(_testFilePath, project);
        var loaded = _service.LoadProject(_testFilePath);

        // Assert
        Assert.That(loaded.Name, Is.EqualTo(project.Name));
        Assert.That(loaded.TotalPrice, Is.EqualTo(project.TotalPrice));
    }
}