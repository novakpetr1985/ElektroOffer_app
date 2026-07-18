using ElektroOffer_app.Invoice.Models;
using ElektroOffer_app.Invoice.Services;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.Invoice;

[TestFixture]
/// <summary>Ověřuje vytvoření, obnovu a odstranění recovery kopie faktury.</summary>
public class InvoiceAutosaveServiceTests
{
    private string _directory = null!;

    [SetUp]
    public void SetUp() => _directory = Path.Combine(Path.GetTempPath(), "ElektroOfferTests", Guid.NewGuid().ToString("N"));

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, recursive: true);
    }

    [Test]
    public void Save_And_LoadLatest_Should_Restore_Draft()
    {
        var service = new InvoiceAutosaveService(_directory);
        var draft = new InvoiceDraft { Number = "2026-RECOVERY" };

        service.Save(draft);
        var restored = service.LoadLatest();

        Assert.Multiple(() =>
        {
            Assert.That(restored, Is.Not.Null);
            Assert.That(restored!.DraftId, Is.EqualTo(draft.DraftId));
            Assert.That(restored.Number, Is.EqualTo("2026-RECOVERY"));
        });
    }

    [Test]
    public void Delete_Should_Remove_Recovery_Draft()
    {
        var service = new InvoiceAutosaveService(_directory);
        var draft = new InvoiceDraft { Number = "2026-DONE" };
        service.Save(draft);

        service.Delete(draft);

        Assert.That(service.LoadLatest(), Is.Null);
    }
}
