using System.IO;
using System.Text.Json;
using ElektroOffer_app.Invoice.Models;

namespace ElektroOffer_app.Invoice.Services;

/// <summary>
/// Atomicky ukládá obnovovací kopie rozpracovaných faktur do uživatelského úložiště.
/// </summary>
public sealed class InvoiceAutosaveService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _directory;

    public InvoiceAutosaveService(string? directory = null)
    {
        _directory = directory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ElektroOffer", "Drafts", "Invoices");
    }

    public void Save(InvoiceDraft draft)
    {
        Directory.CreateDirectory(_directory);
        if (draft.DraftId == Guid.Empty) draft.DraftId = Guid.NewGuid();
        var destination = PathFor(draft.DraftId);
        var temporary = destination + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(draft, JsonOptions));
        File.Move(temporary, destination, overwrite: true);
    }

    public InvoiceDraft? LoadLatest()
    {
        if (!Directory.Exists(_directory)) return null;
        var path = Directory.GetFiles(_directory, "*.eofinvoice.autosave")
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
        if (path == null) return null;
        try { return JsonSerializer.Deserialize<InvoiceDraft>(File.ReadAllText(path), JsonOptions); }
        catch { return null; }
    }

    public void Delete(InvoiceDraft draft)
    {
        var path = PathFor(draft.DraftId);
        if (File.Exists(path)) File.Delete(path);
    }

    private string PathFor(Guid draftId) => Path.Combine(_directory, $"{draftId:N}.eofinvoice.autosave");
}
