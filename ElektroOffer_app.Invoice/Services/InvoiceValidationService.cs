using ElektroOffer_app.Invoice.Models;

namespace ElektroOffer_app.Invoice.Services;

/// <summary>
/// Sbírá upozornění před tiskem nebo exportem faktury, aniž by vypínala příkazy UI.
/// </summary>
public sealed class InvoiceValidationService
{
    public List<string> GetMissingFieldWarnings(InvoiceDraft invoice)
    {
        var warnings = new List<string>();
        if (string.IsNullOrWhiteSpace(invoice.Number)) warnings.Add("Chybí číslo faktury.");
        if (string.IsNullOrWhiteSpace(invoice.Supplier.Name)) warnings.Add("Chybí název dodavatele.");
        if (string.IsNullOrWhiteSpace(invoice.Supplier.Street) || string.IsNullOrWhiteSpace(invoice.Supplier.City)) warnings.Add("Chybí adresa dodavatele.");
        if (string.IsNullOrWhiteSpace(invoice.Supplier.RegistrationNo)) warnings.Add("Chybí IČO dodavatele.");
        if (string.IsNullOrWhiteSpace(invoice.Customer.Name)) warnings.Add("Chybí název odběratele.");
        if (string.IsNullOrWhiteSpace(invoice.Customer.Street) || string.IsNullOrWhiteSpace(invoice.Customer.City)) warnings.Add("Chybí adresa odběratele.");
        if (string.IsNullOrWhiteSpace(invoice.Customer.RegistrationNo)) warnings.Add("Chybí IČO odběratele.");
        if (invoice.DueDays < 0) warnings.Add("Datum splatnosti je před datem vystavení.");
        if (invoice.Lines.Count == 0) warnings.Add("Faktura neobsahuje žádnou položku.");
        if (invoice.Lines.Any(line => string.IsNullOrWhiteSpace(line.Name))) warnings.Add("Některá položka nemá popis.");
        if (invoice.Lines.Any(line => line.Quantity <= 0)) warnings.Add("Některá položka nemá kladné množství.");
        if (invoice.Lines.Any(line => line.UnitPrice < 0 || line.TotalPrice < 0)) warnings.Add("Některá položka obsahuje zápornou cenu.");
        if (invoice.Lines.Any(line => line.VatRate is < 0 or > 100)) warnings.Add("Některá položka obsahuje neplatnou sazbu DPH.");
        if (QrPaymentService.NormalizeIban(invoice.Supplier.Iban) == null)
            warnings.Add("Chybí platný ověřený IBAN; QR platba nebude vygenerována.");
        return warnings;
    }
}
