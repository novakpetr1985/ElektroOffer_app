using ElektroOffer_app.Services;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.Services;

[TestFixture]
/// <summary>Ověřuje umístění DB, bezpečnou adopci legacy souboru a zákaz přepsání.</summary>
public class AppDataPathProviderTests
{
    private string _root = null!;

    [SetUp]
    public void SetUp()
    {
        _root = Path.Combine(Path.GetTempPath(), $"ElektroOffer_Path_{Guid.NewGuid():N}");
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    [Test]
    public void DatabasePath_Should_Create_Missing_User_Directory()
    {
        var dataDirectory = Path.Combine(_root, "missing", "ElektroOffer");
        var provider = new AppDataPathProvider(dataDirectory);

        var path = provider.DatabasePath;

        Assert.Multiple(() =>
        {
            Assert.That(Directory.Exists(dataDirectory), Is.True);
            Assert.That(path, Is.EqualTo(Path.Combine(dataDirectory, "elektrooffer.db")));
        });
    }

    [Test]
    public void DatabasePath_Should_Adopt_Legacy_Database_When_Destination_Is_Missing()
    {
        var legacyPath = Path.Combine(_root, "legacy.db");
        Directory.CreateDirectory(_root);
        File.WriteAllText(legacyPath, "legacy-user-data");
        var provider = new AppDataPathProvider(Path.Combine(_root, "user"), [legacyPath]);

        var path = provider.DatabasePath;

        Assert.That(File.ReadAllText(path), Is.EqualTo("legacy-user-data"));
    }

    [Test]
    public void DatabasePath_Should_Never_Overwrite_Existing_User_Database()
    {
        var dataDirectory = Path.Combine(_root, "user");
        Directory.CreateDirectory(dataDirectory);
        var destination = Path.Combine(dataDirectory, "elektrooffer.db");
        var legacyPath = Path.Combine(_root, "legacy.db");
        File.WriteAllText(destination, "current-user-data");
        File.WriteAllText(legacyPath, "legacy-data");
        var provider = new AppDataPathProvider(dataDirectory, [legacyPath]);

        _ = provider.DatabasePath;

        Assert.That(File.ReadAllText(destination), Is.EqualTo("current-user-data"));
    }
}
