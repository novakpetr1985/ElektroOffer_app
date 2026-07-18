using ElektroOffer_app.Invoice.Models;
using ElektroOffer_app.Invoice.Services;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.Invoice;

[TestFixture]
/// <summary>Ověřuje souhrn upozornění pro neúplnou i kompletní fakturu.</summary>
public class InvoiceValidationServiceTests
{
    private readonly InvoiceValidationService _service = new();

    [Test]
    public void GetMissingFieldWarnings_Should_Report_Empty_Invoice()
    {
        var warnings = _service.GetMissingFieldWarnings(new InvoiceDraft());
        Assert.Multiple(() =>
        {
            Assert.That(warnings, Does.Contain("Chybí číslo faktury."));
            Assert.That(warnings, Does.Contain("Chybí IČO dodavatele."));
            Assert.That(warnings, Does.Contain("Chybí IČO odběratele."));
            Assert.That(warnings, Does.Contain("Faktura neobsahuje žádnou položku."));
        });
    }

    [Test]
    public void GetMissingFieldWarnings_Should_Report_Invalid_Due_Date()
    {
        var invoice = CompleteInvoice();
        invoice.DueDays = -1;
        Assert.That(_service.GetMissingFieldWarnings(invoice), Does.Contain("Datum splatnosti je před datem vystavení."));
    }

    [Test]
    public void GetMissingFieldWarnings_Should_Inform_When_Qr_Is_Unavailable()
    {
        var invoice = CompleteInvoice();
        invoice.Supplier.Iban = string.Empty;
        Assert.That(_service.GetMissingFieldWarnings(invoice), Has.Some.Contains("QR platba"));
    }

    [Test]
    public void GetMissingFieldWarnings_Should_Return_Empty_For_Complete_Invoice()
    {
        Assert.That(_service.GetMissingFieldWarnings(CompleteInvoice()), Is.Empty);
    }

    private static InvoiceDraft CompleteInvoice()
    {
        var invoice = new InvoiceDraft
        {
            Number = "2026-001",
            VariableSymbol = "2026001",
            Supplier = new InvoiceParty { Name = "Dodavatel", Street = "Ulice 1", City = "Praha", RegistrationNo = "27074358", Iban = "CZ6508000000192000145399" },
            Customer = new InvoiceParty { Name = "Odběratel", Street = "Ulice 2", City = "Brno", RegistrationNo = "25596641" }
        };
        invoice.Lines.Add(new InvoiceLine { Name = "Montáž", Quantity = 1, UnitName = "ks", UnitPrice = 100, TotalPrice = 100, VatRate = 21 });
        return invoice;
    }
}
