using System.Globalization;
using System.Text;
using ElektroOffer_app.Invoice.Models;
using QRCoder;

namespace ElektroOffer_app.Invoice.Services;

/// <summary>
/// Validuje platební údaje, sestaví lokální SPAYD payload a vykreslí jej jako PNG QR kód.
/// </summary>
public sealed class QrPaymentService
{
    public string? BuildSpaydString(InvoiceParty supplier, InvoiceDraft invoice)
    {
        // QR používá pouze IBAN potvrzený bankou nebo uživatelem.
        var iban = NormalizeIban(supplier.Iban);
        if (iban == null)
            return null;

        if (!string.Equals(invoice.PaymentMethod, "bank", StringComparison.OrdinalIgnoreCase) ||
            invoice.TotalWithVat <= 0 ||
            !string.Equals(invoice.Currency, "CZK", StringComparison.OrdinalIgnoreCase))
            return null;

        var variableSymbol = invoice.VariableSymbol.Trim();
        if (variableSymbol.Length > 10 || variableSymbol.Any(character => !char.IsDigit(character)))
            return null;
        var message = SanitizeMessage(string.IsNullOrWhiteSpace(invoice.Note)
            ? $"Faktura {invoice.Number}"
            : invoice.Note);

        return string.Join('*',
            "SPD",
            "1.0",
            $"ACC:{iban}",
            $"AM:{invoice.TotalWithVat.ToString("0.00", CultureInfo.InvariantCulture)}",
            $"CC:{invoice.Currency.ToUpperInvariant()}",
            $"DT:{invoice.DueOn:yyyyMMdd}",
            $"X-VS:{variableSymbol}",
            $"MSG:{message}");
    }

    public byte[] GenerateQrPng(string spaydString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(spaydString);
        return PngByteQRCodeHelper.GetQRCode(spaydString, QRCodeGenerator.ECCLevel.M, 10);
    }

    public static string? NormalizeIban(string? value)
    {
        var iban = new string((value ?? string.Empty)
            .Where(character => !char.IsWhiteSpace(character))
            .Select(char.ToUpperInvariant)
            .ToArray());

        return iban.Length == 24 && iban.StartsWith("CZ", StringComparison.Ordinal) &&
               iban.All(char.IsLetterOrDigit) && HasValidMod97(iban)
            ? iban
            : null;
    }

    private static bool HasValidMod97(string iban)
    {
        var rearranged = iban[4..] + iban[..4];
        var remainder = 0;
        foreach (var character in rearranged)
        {
            var numeric = char.IsDigit(character)
                ? character - '0'
                : character - 'A' + 10;
            foreach (var digit in numeric.ToString(CultureInfo.InvariantCulture))
                remainder = (remainder * 10 + digit - '0') % 97;
        }

        return remainder == 1;
    }

    private static string SanitizeMessage(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var result = new StringBuilder();
        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category == UnicodeCategory.NonSpacingMark)
                continue;

            var upper = char.ToUpperInvariant(character);
            result.Append(upper is >= ' ' and <= '~' && upper != '*' ? upper : ' ');
        }

        var compact = string.Join(' ', result.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return compact[..Math.Min(60, compact.Length)];
    }
}
