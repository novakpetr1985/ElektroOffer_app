using ElektroOffer_app.Invoice.Models;
using QuestPDF.Fluent;

namespace ElektroOffer_app.Invoice.Services
{
    /// <summary>
    /// Zapíše InvoiceDocument do cílového PDF souboru.
    /// </summary>
    public class PdfInvoiceExportService
    {
        public void Export(string path, InvoiceDraft draft)
        {
            new InvoiceDocument(draft).GeneratePdf(path);
        }
    }
}
