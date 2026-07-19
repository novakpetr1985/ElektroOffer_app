using System.Text.Json;
using ElektroOffer_app.Invoice.Models;
using ElektroOffer_app.Invoice.Services;
using ElektroOffer_app.Invoice.ViewModels;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.Invoice
{
    [TestFixture]
    /// <summary>Regresní testy serializace, klonování, Fakturoid exportu a základního PDF.</summary>
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
        public void Invoice_Should_Preserve_Before_Discount_Discount_And_Final_Prices()
        {
            var vm = new InvoiceViewModel(new[]
            {
                new InvoiceSourceItem
                {
                    Type = "PRACE",
                    Description = "Montaz",
                    Unit = "ks",
                    Quantity = 2,
                    PriceBeforeDiscount = 960,
                    Price = 864,
                    DiscountPercent = 10,
                    DiscountAmount = 96
                }
            });

            var line = vm.Lines.Single();
            Assert.Multiple(() =>
            {
                Assert.That(line.UnitPrice, Is.EqualTo(480m));
                Assert.That(line.TotalPriceBeforeDiscount, Is.EqualTo(960m));
                Assert.That(line.DiscountPercent, Is.EqualTo(10d));
                Assert.That(line.DiscountAmount, Is.EqualTo(96m));
                Assert.That(line.TotalPrice, Is.EqualTo(864m));
                Assert.That(vm.Total, Is.EqualTo(864m));
            });
        }

        [Test]
        public void ClearSupplier_Should_ClearOnlySupplierSection()
        {
            var draft = CreateDraft();
            draft.Supplier.AccountNumber = "123456789";
            var vm = new InvoiceViewModel([], draft);

            vm.ClearSupplierCommand.Execute(null);

            Assert.Multiple(() =>
            {
                Assert.That(vm.Supplier.Name, Is.Empty);
                Assert.That(vm.Supplier.RegistrationNo, Is.Empty);
                Assert.That(vm.Supplier.AccountNumber, Is.Empty);
                Assert.That(vm.Supplier.Country, Is.EqualTo("CZ"));
                Assert.That(vm.Customer.Name, Is.EqualTo("Test Customer s.r.o."));
                Assert.That(vm.Lines, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public void ClearCustomer_Should_ClearOnlyCustomerSection()
        {
            var draft = CreateDraft();
            draft.Supplier.Name = "Test Supplier s.r.o.";
            var vm = new InvoiceViewModel([], draft);

            vm.ClearCustomerCommand.Execute(null);

            Assert.Multiple(() =>
            {
                Assert.That(vm.Customer.Name, Is.Empty);
                Assert.That(vm.Customer.RegistrationNo, Is.Empty);
                Assert.That(vm.Supplier.Name, Is.EqualTo("Test Supplier s.r.o."));
                Assert.That(vm.Lines, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public void MeaningfulContent_Should_DetectFilledPartyOrInvoiceLines()
        {
            var empty = new InvoiceDraft();
            var partyOnly = new InvoiceDraft();
            partyOnly.Customer.RegistrationNo = "27074358";

            Assert.Multiple(() =>
            {
                Assert.That(InvoiceDraftStateService.HasMeaningfulContent(empty), Is.False);
                Assert.That(InvoiceDraftStateService.HasMeaningfulContent(partyOnly), Is.True);
                Assert.That(InvoiceDraftStateService.HasMeaningfulContent(CreateDraft()), Is.True);
            });
        }

        [Test]
        public void FakturoidExport_Should_Use_Effective_Unit_Price_After_Discount()
        {
            var draft = CreateDraft();
            var line = draft.Lines[0];
            line.UnitPrice = 480m;
            line.TotalPriceBeforeDiscount = 960m;
            line.TotalPrice = 864m;
            line.Quantity = 2;
            line.DiscountPercent = 10;
            line.DiscountAmount = 96m;

            var json = new FakturoidExportService().BuildJson(draft);
            using var document = JsonDocument.Parse(json);

            Assert.That(
                document.RootElement.GetProperty("lines")[0].GetProperty("unit_price").GetString(),
                Is.EqualTo("432"));
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
                TotalPriceBeforeDiscount = 3000,
                TotalPrice = 3000,
                VatRate = 21
            });

            return draft;
        }
    }
}
