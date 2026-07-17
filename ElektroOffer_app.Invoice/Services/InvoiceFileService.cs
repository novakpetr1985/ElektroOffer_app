using System.IO;
using System.Text.Json;
using ElektroOffer_app.Invoice.Models;

namespace ElektroOffer_app.Invoice.Services
{
    /// <summary>
    /// Ukládá a načítá samostatné přenositelné soubory faktur ve formátu JSON.
    /// </summary>
    public class InvoiceFileService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public void Save(string path, InvoiceDraft draft)
        {
            var json = JsonSerializer.Serialize(draft, JsonOptions);
            File.WriteAllText(path, json);
        }

        public InvoiceDraft? Load(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<InvoiceDraft>(json, JsonOptions);
        }
    }
}
