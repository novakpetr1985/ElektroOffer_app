using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using ElektroOffer_app.Invoice.Models;

namespace ElektroOffer_app.Invoice.Services
{
    public class PdfInvoiceExportService
    {
        public void Export(string path, InvoiceDraft draft)
        {
            var lines = BuildTextLines(draft);
            var content = BuildPageContent(lines);
            var pdf = BuildPdf(content);
            File.WriteAllBytes(path, pdf);
        }

        private static List<string> BuildTextLines(InvoiceDraft draft)
        {
            var lines = new List<string>
            {
                "FAKTURA",
                "",
                $"Cislo: {draft.Number}",
                $"Variabilni symbol: {draft.VariableSymbol}",
                $"Vystaveno: {draft.IssuedOn:yyyy-MM-dd}",
                $"Splatnost: {draft.IssuedOn.AddDays(draft.DueDays):yyyy-MM-dd}",
                "",
                $"Dodavatel: {draft.Supplier.Name}",
                $"{draft.Supplier.RegistrationNo} {draft.Supplier.VatNo}".Trim(),
                $"{draft.Supplier.Street}, {draft.Supplier.Zip} {draft.Supplier.City}".Trim(' ', ','),
                "",
                $"Odberatel: {draft.Customer.Name}",
                $"{draft.Customer.RegistrationNo} {draft.Customer.VatNo}".Trim(),
                $"{draft.Customer.Street}, {draft.Customer.Zip} {draft.Customer.City}".Trim(' ', ','),
                "",
                "Polozky:"
            };

            foreach (var line in draft.Lines)
            {
                lines.Add($"{line.Name} | {line.Quantity.ToString("0.####", CultureInfo.InvariantCulture)} {line.UnitName} | {line.TotalPrice:N2} {draft.Currency}");
            }

            lines.Add("");
            lines.Add($"Celkem: {draft.Total:N2} {draft.Currency}");

            if (!string.IsNullOrWhiteSpace(draft.Note))
            {
                lines.Add("");
                lines.Add("Poznamka:");
                lines.AddRange(Wrap(draft.Note, 90));
            }

            return lines;
        }

        private static string BuildPageContent(IReadOnlyList<string> lines)
        {
            var sb = new StringBuilder();
            sb.AppendLine("BT");
            var fontSize = Token("Typography.FontSize.Document", 10d);
            var pagePadding = Token("Spacing.DocumentPagePadding", new Thickness(50));
            sb.AppendLine($"/F1 {fontSize.ToString(CultureInfo.InvariantCulture)} Tf");
            sb.AppendLine($"{pagePadding.Left.ToString(CultureInfo.InvariantCulture)} {(842 - pagePadding.Top).ToString(CultureInfo.InvariantCulture)} Td");

            foreach (var line in lines.Take(48))
            {
                sb.Append('(').Append(EscapePdf(ToAscii(line))).AppendLine(") Tj");
                sb.AppendLine($"0 {(-fontSize * 1.5d).ToString(CultureInfo.InvariantCulture)} Td");
            }

            sb.AppendLine("ET");
            return sb.ToString();
        }

        private static T Token<T>(string key, T fallback)
            => Application.Current?.TryFindResource(key) is T value ? value : fallback;

        private static byte[] BuildPdf(string content)
        {
            var objects = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
                $"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}endstream"
            };

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true);
            writer.WriteLine("%PDF-1.4");

            var offsets = new List<long> { 0 };
            for (int i = 0; i < objects.Count; i++)
            {
                writer.Flush();
                offsets.Add(stream.Position);
                writer.WriteLine($"{i + 1} 0 obj");
                writer.WriteLine(objects[i]);
                writer.WriteLine("endobj");
            }

            writer.Flush();
            var xrefPosition = stream.Position;
            writer.WriteLine("xref");
            writer.WriteLine($"0 {objects.Count + 1}");
            writer.WriteLine("0000000000 65535 f ");
            foreach (var offset in offsets.Skip(1))
                writer.WriteLine($"{offset:0000000000} 00000 n ");

            writer.WriteLine("trailer");
            writer.WriteLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
            writer.WriteLine("startxref");
            writer.WriteLine(xrefPosition);
            writer.WriteLine("%%EOF");
            writer.Flush();

            return stream.ToArray();
        }

        private static IEnumerable<string> Wrap(string text, int maxLength)
        {
            var current = text;
            while (current.Length > maxLength)
            {
                yield return current[..maxLength];
                current = current[maxLength..];
            }

            if (current.Length > 0)
                yield return current;
        }

        private static string EscapePdf(string value)
            => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

        private static string ToAscii(string value)
        {
            const string source = "áčďéěíňóřšťúůýžÁČĎÉĚÍŇÓŘŠŤÚŮÝŽ";
            const string target = "acdeeinorstuuyzACDEEINORSTUUYZ";
            var result = new StringBuilder(value.Length);

            foreach (var ch in value)
            {
                var index = source.IndexOf(ch);
                if (index >= 0)
                    result.Append(target[index]);
                else
                    result.Append(ch < 128 ? ch : '?');
            }

            return result.ToString();
        }
    }
}
