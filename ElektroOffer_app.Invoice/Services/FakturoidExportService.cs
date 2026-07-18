using System.Globalization;
using System.Text.Json;
using ElektroOffer_app.Invoice.Models;

namespace ElektroOffer_app.Invoice.Services
{
    /// <summary>
    /// Mapuje interní návrh faktury do JSON payloadu kompatibilního s Fakturoid API.
    /// </summary>
    public class FakturoidExportService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public string BuildJson(InvoiceDraft draft)
        {
            var payload = new
            {
                custom_id = string.IsNullOrWhiteSpace(draft.Number) ? null : draft.Number,
                document_type = "invoice",
                number = string.IsNullOrWhiteSpace(draft.Number) ? null : draft.Number,
                variable_symbol = string.IsNullOrWhiteSpace(draft.VariableSymbol) ? null : draft.VariableSymbol,
                issued_on = draft.IssuedOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                due = draft.DueDays,
                currency = draft.Currency,
                language = "cz",
                payment_method = draft.PaymentMethod,
                client_name = EmptyToNull(draft.Customer.Name),
                client_street = EmptyToNull(draft.Customer.Street),
                client_city = EmptyToNull(draft.Customer.City),
                client_zip = EmptyToNull(draft.Customer.Zip),
                client_country = EmptyToNull(draft.Customer.Country),
                client_registration_no = EmptyToNull(draft.Customer.RegistrationNo),
                client_vat_no = EmptyToNull(draft.Customer.VatNo),
                note = EmptyToNull(draft.Note),
                footer_note = EmptyToNull(draft.FooterNote),
                vat_price_mode = "without_vat",
                lines = draft.Lines.Select(line => new
                {
                    name = line.Name,
                    quantity = line.Quantity.ToString("0.####", CultureInfo.InvariantCulture),
                    unit_name = line.UnitName,
                    unit_price = (line.Quantity > 0
                            ? line.TotalPrice / Convert.ToDecimal(line.Quantity)
                            : line.TotalPrice)
                        .ToString("0.##", CultureInfo.InvariantCulture),
                    vat_rate = line.VatRate
                }).ToList()
            };

            return JsonSerializer.Serialize(payload, JsonOptions);
        }

        private static string? EmptyToNull(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
