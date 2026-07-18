using ElektroOffer_app.Invoice.Models;
using ElektroOffer_app.Invoice.Services;
using ElektroOffer_app.Models;
using NUnit.Framework;
using System.Text.Json;

namespace ElektroOffer_app.Tests.Integration.Invoice
{
    [TestFixture]
    /// <summary>Ověřuje samostatné uložení a načtení faktury přes skutečný soubor.</summary>
    public class InvoiceFileIntegrationTests
    {
        [Test]
        public void InvoiceFileService_Should_Save_And_Load_Invoice_Draft()
        {
            var path = Path.Combine(Path.GetTempPath(), $"invoice_{Guid.NewGuid()}.eofinvoice");
            var service = new InvoiceFileService();
            var draft = CreateDraft();

            try
            {
                service.Save(path, draft);
                var loaded = service.Load(path);

                Assert.That(loaded, Is.Not.Null);
                Assert.That(loaded!.Customer.Name, Is.EqualTo("Zakaznik"));
                Assert.That(loaded.Lines, Has.Count.EqualTo(1));
                Assert.That(loaded.Lines[0].TotalPriceBeforeDiscount, Is.EqualTo(3000m));
                Assert.That(loaded.Lines[0].DiscountPercent, Is.EqualTo(10d));
                Assert.That(loaded.Lines[0].DiscountAmount, Is.EqualTo(500m));
                Assert.That(loaded.Lines[0].TotalPrice, Is.EqualTo(2500m));
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        [Test]
        public void ProjectData_Should_Serialize_Invoice_Draft_And_Row_Counts()
        {
            var project = new ProjectData
            {
                ProjectName = "Projekt s fakturaci",
                WorkRowCount = 7,
                MaterialRowCount = 3,
                InvoiceDraft = CreateDraft()
            };

            var json = JsonSerializer.Serialize(project);
            var loaded = JsonSerializer.Deserialize<ProjectData>(json);

            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded!.WorkRowCount, Is.EqualTo(7));
            Assert.That(loaded.MaterialRowCount, Is.EqualTo(3));
            Assert.That(loaded.InvoiceDraft, Is.Not.Null);
            Assert.That(loaded.InvoiceDraft!.Customer.RegistrationNo, Is.EqualTo("27074358"));
            Assert.That(loaded.InvoiceDraft.Lines[0].Name, Is.EqualTo("Material: Kabel"));
        }

        private static InvoiceDraft CreateDraft()
        {
            var draft = new InvoiceDraft
            {
                Customer = new InvoiceParty
                {
                    Name = "Zakaznik",
                    RegistrationNo = "27074358"
                }
            };

            draft.Lines.Add(new InvoiceLine
            {
                Type = "Material",
                Name = "Material: Kabel",
                Quantity = 5,
                UnitName = "m",
                UnitPrice = 500,
                TotalPriceBeforeDiscount = 3000,
                DiscountPercent = 10,
                DiscountAmount = 500,
                TotalPrice = 2500
            });

            return draft;
        }
    }
}
