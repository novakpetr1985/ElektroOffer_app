using ElektroOffer_app.Invoice.Models;
using ElektroOffer_app.Invoice.Services;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.Invoice;

[TestFixture]
/// <summary>Ověřuje SPAYD payload, podmínky QR platby a lokální PNG výstup.</summary>
public class QrPaymentServiceTests
{
    private readonly QrPaymentService _service = new();

    [Test]
    public void SpaydPayload_Should_Create_Expected_Czech_Payment()
    {
        var invoice = CompleteInvoice();

        var payload = _service.BuildSpaydString(invoice.Supplier, invoice);

        Assert.That(payload, Is.EqualTo("SPD*1.0*ACC:CZ6508000000192000145399*AM:121.00*CC:CZK*DT:20260730*X-VS:2026001*MSG:UHRADA FAKTURY C. 2026-001"));
    }

    [Test]
    public void BuildSpaydString_Should_Return_Null_Without_Valid_Iban()
    {
        var invoice = CompleteInvoice();
        invoice.Supplier.Iban = "";
        Assert.That(_service.BuildSpaydString(invoice.Supplier, invoice), Is.Null);
    }

    [TestCase("cash")]
    [TestCase("card")]
    public void BuildSpaydString_Should_Return_Null_Outside_Bank_Transfer(string paymentMethod)
    {
        var invoice = CompleteInvoice();
        invoice.PaymentMethod = paymentMethod;
        Assert.That(_service.BuildSpaydString(invoice.Supplier, invoice), Is.Null);
    }

    [Test]
    public void BuildSpaydString_Should_Return_Null_For_Invalid_Variable_Symbol()
    {
        var invoice = CompleteInvoice();
        invoice.VariableSymbol = "2026-A";
        Assert.That(_service.BuildSpaydString(invoice.Supplier, invoice), Is.Null);
    }

    [Test]
    public void GenerateQrPng_Should_Create_Local_Png()
    {
        var bytes = _service.GenerateQrPng("SPD*1.0*ACC:CZ6508000000192000145399*AM:1.00*CC:CZK");
        Assert.Multiple(() =>
        {
            Assert.That(bytes.Length, Is.GreaterThan(500));
            Assert.That(bytes.Take(8), Is.EqualTo(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
        });
    }

    private static InvoiceDraft CompleteInvoice()
    {
        var invoice = new InvoiceDraft
        {
            Number = "2026-001",
            VariableSymbol = "2026001",
            IssuedOn = new DateTime(2026, 7, 16),
            DueDays = 14,
            Currency = "CZK",
            PaymentMethod = "bank",
            Note = "Úhrada faktury č. 2026-001",
            Supplier = new InvoiceParty { Iban = "CZ65 0800 0000 1920 0014 5399" }
        };
        invoice.Lines.Add(new InvoiceLine { Name = "Práce", Quantity = 1, UnitName = "ks", TotalPrice = 100, VatRate = 21 });
        return invoice;
    }
}
