using System.Text.Json;
using ElektroOffer_app.Invoice.Models;
using ElektroOffer_app.Invoice.Services;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.Invoice
{
    [TestFixture]
    public class InvoiceServicesTests
    {
        [Test]
        public void FakturoidExport_Should_Build_Expected_Client_And_Line_Json()
        {
            var draft = CreateDraft();
            var service = new FakturoidExportService();

            var json = service.BuildJson(draft);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            Assert.That(root.GetProperty("client_name").GetString(), Is.EqualTo("Test Customer s.r.o."));
            Assert.That(root.GetProperty("client_registration_no").GetString(), Is.EqualTo("27074358"));
            Assert.That(root.GetProperty("client_vat_no").GetString(), Is.EqualTo("CZ27074358"));
            Assert.That(root.GetProperty("lines")[0].GetProperty("name").GetString(), Is.EqualTo("PRACE: Montaz"));
            Assert.That(root.GetProperty("lines")[0].GetProperty("quantity").GetString(), Is.EqualTo("2"));
            Assert.That(root.GetProperty("lines")[0].GetProperty("unit_price").GetString(), Is.EqualTo("1500"));
            Assert.That(root.GetProperty("lines")[0].GetProperty("vat_rate").GetDecimal(), Is.EqualTo(21));
        }

        [Test]
        public void InvoiceDraftClone_Should_Create_Independent_Copy()
        {
            var original = CreateDraft();

            var clone = InvoiceDraftCloneService.Clone(original);
            clone.Customer.Name = "Changed";
            clone.Lines[0].Name = "Changed line";

            Assert.That(original.Customer.Name, Is.EqualTo("Test Customer s.r.o."));
            Assert.That(original.Lines[0].Name, Is.EqualTo("PRACE: Montaz"));
            Assert.That(clone.Customer.Name, Is.EqualTo("Changed"));
        }

        [Test]
        public void AresLookup_Should_Accept_Only_Valid_Czech_Registration_Number()
        {
            Assert.That(AresLookupService.NormalizeRegistrationNo("27074358"), Is.EqualTo("27074358"));
            Assert.That(AresLookupService.NormalizeRegistrationNo("270 743 58"), Is.EqualTo("27074358"));
            Assert.That(AresLookupService.NormalizeRegistrationNo("27074359"), Is.Null);
            Assert.That(AresLookupService.NormalizeRegistrationNo("123"), Is.Null);
        }

        [Test]
        public void PdfExport_Should_Create_NonEmpty_Pdf_File()
        {
            var path = Path.Combine(Path.GetTempPath(), $"invoice_{Guid.NewGuid()}.pdf");
            var service = new PdfInvoiceExportService();

            try
            {
                service.Export(path, CreateDraft());

                var bytes = File.ReadAllBytes(path);
                Assert.That(bytes.Length, Is.GreaterThan(100));
                Assert.That(System.Text.Encoding.ASCII.GetString(bytes.Take(8).ToArray()), Is.EqualTo("%PDF-1.4"));
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        private static InvoiceDraft CreateDraft()
        {
            var draft = new InvoiceDraft
            {
                Number = "FV-2026-001",
                VariableSymbol = "2026001",
                DueDays = 14,
                Currency = "CZK",
                Customer = new InvoiceParty
                {
                    Name = "Test Customer s.r.o.",
                    RegistrationNo = "27074358",
                    VatNo = "CZ27074358",
                    Street = "Testovaci 1",
                    City = "Praha",
                    Zip = "11000"
                }
            };

            draft.Lines.Add(new InvoiceLine
            {
                Type = "PRACE",
                Name = "PRACE: Montaz",
                Quantity = 2,
                UnitName = "hod",
                UnitPrice = 1500,
                TotalPrice = 3000,
                VatRate = 21
            });

            return draft;
        }
    }
}
