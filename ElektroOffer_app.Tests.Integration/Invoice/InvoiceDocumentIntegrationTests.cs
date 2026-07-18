using ElektroOffer_app.Invoice.Models;
using ElektroOffer_app.Invoice.Services;
using NUnit.Framework;
using QuestPDF.Fluent;

namespace ElektroOffer_app.Tests.Integration.Invoice;

[TestFixture]
/// <summary>Renderuje ProfessionalA4 s různým rozsahem a ověřuje PDF, QR a stránkování.</summary>
public class InvoiceDocumentIntegrationTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void ProfessionalA4_Should_Render_With_And_Without_Qr(bool withQr)
    {
        var invoice = CreateInvoice(3, withQr);
        var document = new InvoiceDocument(invoice);

        var pdf = document.GeneratePdf();

        Assert.Multiple(() =>
        {
            Assert.That(pdf.Length, Is.GreaterThan(10_000));
            Assert.That(document.HasQrPayment, Is.EqualTo(withQr));
        });
    }

    [Test]
    public void ProfessionalA4_Should_Create_Multiple_Pages_For_Long_Invoice()
    {
        var document = new InvoiceDocument(CreateInvoice(100, true));

        var pages = document.GenerateImages().ToList();

        Assert.That(pages, Has.Count.GreaterThan(1));
        Assert.That(pages.All(page => page.Length > 1_000), Is.True);
    }

    private static InvoiceDraft CreateInvoice(int lineCount, bool withQr)
    {
        var invoice = new InvoiceDraft
        {
            Number = "2026-001",
            VariableSymbol = "2026001",
            IssuedOn = new DateTime(2026, 7, 16),
            TaxableSupplyOn = new DateTime(2026, 7, 16),
            Supplier = new InvoiceParty { Name = "Elektro Dodavatel s.r.o.", Street = "Dlouhá 1", City = "Praha", Zip = "11000", RegistrationNo = "27074358", VatNo = "CZ27074358", Iban = withQr ? "CZ6508000000192000145399" : "" },
            Customer = new InvoiceParty { Name = "Český odběratel s.r.o.", Street = "Krátká 2", City = "Brno", Zip = "60200", RegistrationNo = "25596641" },
            Note = "Děkujeme za spolupráci."
        };
        for (var index = 1; index <= lineCount; index++)
            invoice.Lines.Add(new InvoiceLine { Name = $"Položka {index}: odborná elektroinstalační práce s delším českým popisem", Quantity = 2, UnitName = "hod", UnitPrice = 500, TotalPriceBeforeDiscount = 1_000, TotalPrice = 900, DiscountPercent = 10, DiscountAmount = 100, VatRate = 21 });
        return invoice;
    }
}
