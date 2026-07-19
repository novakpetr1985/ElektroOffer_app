using ElektroOffer_app.Invoice.Models;

namespace ElektroOffer_app.Invoice.Services;

public static class InvoiceDraftStateService
{
    public static void ClearParty(InvoiceParty party)
    {
        ArgumentNullException.ThrowIfNull(party);

        party.Name = string.Empty;
        party.RegistrationNo = string.Empty;
        party.VatNo = string.Empty;
        party.Street = string.Empty;
        party.City = string.Empty;
        party.Zip = string.Empty;
        party.Country = "CZ";
        party.Email = string.Empty;
        party.Phone = string.Empty;
        party.AccountPrefix = string.Empty;
        party.AccountNumber = string.Empty;
        party.BankCode = string.Empty;
        party.Iban = string.Empty;
        party.Swift = string.Empty;
    }

    public static bool HasMeaningfulContent(InvoiceDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        return draft.Lines.Count > 0
            || HasContent(draft.Supplier)
            || HasContent(draft.Customer)
            || HasText(draft.Number, draft.VariableSymbol, draft.LogoPath, draft.OrderNumber, draft.ProjectNumber);
    }

    private static bool HasContent(InvoiceParty party) =>
        HasText(
            party.Name, party.RegistrationNo, party.VatNo, party.Street, party.City, party.Zip,
            party.Email, party.Phone, party.AccountPrefix, party.AccountNumber, party.BankCode,
            party.Iban, party.Swift);

    private static bool HasText(params string[] values) =>
        values.Any(value => !string.IsNullOrWhiteSpace(value));
}
