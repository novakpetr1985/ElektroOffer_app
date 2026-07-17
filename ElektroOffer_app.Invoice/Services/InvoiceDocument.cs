using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using ElektroOffer_app.Invoice.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ElektroOffer_app.Invoice.Services;

/// <summary>
/// Sestavuje profesionální vícestránkovou fakturu ProfessionalA4 z jednoho InvoiceDraftu.
/// </summary>
public sealed class InvoiceDocument : IDocument
{
    private static readonly CultureInfo CzechCulture = CultureInfo.GetCultureInfo("cs-CZ");
    private readonly InvoiceDraft _invoice;
    private readonly byte[]? _qrCode;
    private readonly string _accent = TokenColor("Color.Accent", "#2563EB");
    private readonly string _border = TokenColor("Color.Border", "#D6DDE6");
    private readonly string _text = TokenColor("Color.Text", "#1F2937");
    private readonly string _muted = TokenColor("Color.TextMuted", "#667085");
    private readonly float _fontSize = (float)TokenDouble("Typography.FontSize.Document", 10);
    private readonly float _pageMargin = (float)TokenPageMargin("Spacing.DocumentPagePadding", 50);
    private readonly string _fontFamily = TokenFontFamily("Typography.FontFamily.Document", "Arial");

    static InvoiceDocument()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public InvoiceDocument(InvoiceDraft invoice, QrPaymentService? qrPaymentService = null)
    {
        _invoice = invoice;
        var qrService = qrPaymentService ?? new QrPaymentService();
        var spayd = qrService.BuildSpaydString(invoice.Supplier, invoice);
        _qrCode = spayd == null ? null : qrService.GenerateQrPng(spayd);
    }

    public bool HasQrPayment => _qrCode != null;

    public DocumentMetadata GetMetadata() => new()
    {
        Title = $"Faktura {_invoice.Number}",
        Author = _invoice.Supplier.Name,
        Subject = "Faktura ElektroOffer"
    };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(_pageMargin);
            page.DefaultTextStyle(style => style.FontFamily(_fontFamily).FontSize(_fontSize).FontColor(_text));
            page.Header().Element(ComposeHeader);
            page.Content().PaddingVertical(16).Element(ComposeContent);
            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("ElektroOffer • strana ").FontColor(_muted);
                text.CurrentPageNumber().FontColor(_muted);
                text.Span(" / ").FontColor(_muted);
                text.TotalPages().FontColor(_muted);
            });
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Row(left =>
            {
                var logo = ReadLogo();
                if (logo != null)
                    left.ConstantItem(72).Height(42).Image(logo).FitArea();
                else
                    left.ConstantItem(42).Height(42).Background(_accent).AlignCenter().AlignMiddle().Text("EO").Bold().FontSize(16).FontColor(QuestPDF.Helpers.Colors.White);

                left.RelativeItem().PaddingLeft(12).Column(column =>
                {
                    column.Item().Text("FAKTURA").Bold().FontSize(22).FontColor(_accent);
                    column.Item().Text("DAŇOVÝ DOKLAD").SemiBold().FontSize(9).FontColor(_muted);
                    column.Item().Text(_invoice.Supplier.Name).SemiBold();
                });
            });

            row.ConstantItem(190).AlignRight().Column(column =>
            {
                MetadataLine(column, "Číslo", _invoice.Number);
                MetadataLine(column, "Variabilní symbol", _invoice.VariableSymbol);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.Column(column =>
        {
            column.Spacing(14);
            column.Item().Element(ComposeParties);
            column.Item().Element(ComposeMetadata);
            column.Item().Element(ComposeItems);
            column.Item().ShowEntire().Element(ComposeSummary);
            column.Item().ShowEntire().Element(ComposePayment);
            column.Item().ShowEntire().Element(ComposeNotesAndSignature);
        });
    }

    private void ComposeParties(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Element(c => PartyCard(c, "DODAVATEL", _invoice.Supplier));
            row.ConstantItem(14);
            row.RelativeItem().Element(c => PartyCard(c, "ODBĚRATEL", _invoice.Customer));
        });
    }

    private void PartyCard(IContainer container, string title, InvoiceParty party)
    {
        container.Border(1).BorderColor(_border).Padding(12).Column(column =>
        {
            column.Item().Text(title).Bold().FontColor(_accent);
            column.Item().PaddingTop(5).Text(party.Name).SemiBold();
            column.Item().Text(party.Street);
            column.Item().Text($"{party.Zip} {party.City}".Trim());
            column.Item().PaddingTop(4).Text($"IČO: {party.RegistrationNo}  DIČ: {party.VatNo}".Trim());
            if (!string.IsNullOrWhiteSpace(party.Email)) column.Item().Text(party.Email).FontColor(_muted);
            if (!string.IsNullOrWhiteSpace(party.Phone)) column.Item().Text(party.Phone).FontColor(_muted);
        });
    }

    private void ComposeMetadata(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Background("#F5F7FA").Padding(10).Row(row =>
            {
                row.RelativeItem().Column(c => MetadataLine(c, "Datum vystavení", _invoice.IssuedOn.ToString("d.M.yyyy", CzechCulture)));
                row.RelativeItem().Column(c => MetadataLine(c, "DUZP", _invoice.TaxableSupplyOn.ToString("d.M.yyyy", CzechCulture)));
                row.RelativeItem().Column(c => MetadataLine(c, "Datum splatnosti", _invoice.DueOn.ToString("d.M.yyyy", CzechCulture)));
                row.RelativeItem().Column(c => MetadataLine(c, "Forma úhrady", _invoice.PaymentMethod));
            });
            column.Item().PaddingTop(6).Row(row =>
            {
                row.RelativeItem().Column(c => MetadataLine(c, "Číslo objednávky", _invoice.OrderNumber));
                row.RelativeItem().Column(c => MetadataLine(c, "Číslo zakázky", _invoice.ProjectNumber));
            });
        });
    }

    private void ComposeItems(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(22);
                columns.RelativeColumn(2.8f);
                columns.ConstantColumn(46);
                columns.ConstantColumn(57);
                columns.ConstantColumn(54);
                columns.ConstantColumn(34);
                columns.ConstantColumn(58);
                columns.ConstantColumn(52);
                columns.ConstantColumn(62);
            });

            table.Header(header =>
            {
                HeaderCell(header, "#"); HeaderCell(header, "Položka"); HeaderCell(header, "Množ./MJ");
                HeaderCell(header, "Cena/MJ"); HeaderCell(header, "Sleva"); HeaderCell(header, "DPH");
                HeaderCell(header, "Základ"); HeaderCell(header, "DPH Kč"); HeaderCell(header, "Celkem");
            });

            var index = 1;
            foreach (var line in _invoice.Lines)
            {
                var vat = Math.Round(line.TotalPrice * line.VatRate / 100m, 2);
                BodyCell(table, index++.ToString(CzechCulture)); BodyCell(table, line.Name, false);
                BodyCell(table, $"{line.Quantity:0.##} {line.UnitName}"); BodyCell(table, Money(line.UnitPrice));
                BodyCell(table, line.DiscountPercent.HasValue ? $"{line.DiscountPercent:0.##}%\n{Money(line.DiscountAmount ?? 0)}" : "—");
                BodyCell(table, $"{line.VatRate:0.##}%"); BodyCell(table, Money(line.TotalPrice));
                BodyCell(table, Money(vat)); BodyCell(table, Money(line.TotalPrice + vat));
            }
        });
    }

    private void ComposeSummary(IContainer container)
    {
        container.AlignRight().Width(245).BorderTop(1).BorderColor(_border).PaddingTop(8).Column(column =>
        {
            foreach (var group in _invoice.Lines.GroupBy(line => line.VatRate).OrderBy(group => group.Key))
            {
                var basis = group.Sum(line => line.TotalPrice);
                var vat = group.Sum(line => Math.Round(line.TotalPrice * line.VatRate / 100m, 2));
                SummaryLine(column, $"Základ {group.Key:0.##} %", basis);
                SummaryLine(column, $"DPH {group.Key:0.##} %", vat);
            }
            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text("CELKEM").Bold().FontSize(13);
                row.ConstantItem(120).AlignRight().Text($"{Money(_invoice.TotalWithVat)} {_invoice.Currency}").Bold().FontSize(13).FontColor(_accent);
            });
        });
    }

    private void ComposePayment(IContainer container)
    {
        container.Border(1).BorderColor(_border).Padding(12).Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("PLATEBNÍ ÚDAJE").Bold().FontColor(_accent);
                column.Item().PaddingTop(5).Text($"Účet: {DomesticAccount(_invoice.Supplier)}");
                column.Item().Text($"IBAN: {_invoice.Supplier.Iban}");
                if (!string.IsNullOrWhiteSpace(_invoice.Supplier.Swift)) column.Item().Text($"BIC/SWIFT: {_invoice.Supplier.Swift}");
                column.Item().Text($"Variabilní symbol: {_invoice.VariableSymbol}");
                column.Item().PaddingTop(8).Text("ČÁSTKA K ÚHRADĚ").FontSize(8).FontColor(_muted);
                column.Item().Text($"{Money(_invoice.TotalWithVat)} {_invoice.Currency}").Bold().FontSize(15).FontColor(_accent);
                if (_qrCode == null) column.Item().PaddingTop(5).Text("QR platba není dostupná – doplňte platný ověřený IBAN.").FontColor(_muted);
            });
            if (_qrCode != null) row.ConstantItem(105).Height(105).Image(_qrCode).FitArea();
        });
    }

    private void ComposeNotesAndSignature(IContainer container)
    {
        container.Column(column =>
        {
            if (!string.IsNullOrWhiteSpace(_invoice.Note)) column.Item().Text($"Poznámka: {_invoice.Note}");
            if (!string.IsNullOrWhiteSpace(_invoice.FooterNote)) column.Item().PaddingTop(4).Text(_invoice.FooterNote).FontColor(_muted);
            column.Item().PaddingTop(30).AlignRight().Width(180).BorderTop(1).BorderColor(_border).PaddingTop(5).AlignCenter().Text("Podpis a razítko").FontColor(_muted);
        });
    }

    private void HeaderCell(TableCellDescriptor cells, string text) => cells.Cell().Background(_accent).Padding(5).AlignCenter().Text(text).SemiBold().FontSize(8).FontColor(QuestPDF.Helpers.Colors.White);
    private void BodyCell(TableDescriptor table, string text, bool alignRight = true) => table.Cell().ShowEntire().BorderBottom(1).BorderColor(_border).Padding(5).Element(c => alignRight ? c.AlignRight() : c).Text(text).FontSize(8);
    private void SummaryLine(ColumnDescriptor column, string label, decimal value) => column.Item().Row(row => { row.RelativeItem().Text(label); row.ConstantItem(120).AlignRight().Text($"{Money(value)} {_invoice.Currency}"); });
    private void MetadataLine(ColumnDescriptor column, string label, string value) { column.Item().Text(label).FontSize(8).FontColor(_muted); column.Item().Text(string.IsNullOrWhiteSpace(value) ? "—" : value).SemiBold(); }
    private static string Money(decimal value) => value.ToString("N2", CzechCulture);
    private static string DomesticAccount(InvoiceParty party) => string.IsNullOrWhiteSpace(party.AccountNumber) ? "—" : $"{(string.IsNullOrWhiteSpace(party.AccountPrefix) ? "" : party.AccountPrefix + "-")}{party.AccountNumber}/{party.BankCode}";
    private byte[]? ReadLogo() { try { return File.Exists(_invoice.LogoPath) ? File.ReadAllBytes(_invoice.LogoPath) : null; } catch { return null; } }
    private static object? Token(string key) => Application.Current?.TryFindResource(key);
    private static string TokenColor(string key, string fallback) => Token(key) is System.Windows.Media.Color color ? $"#{color.R:X2}{color.G:X2}{color.B:X2}" : fallback;
    private static double TokenDouble(string key, double fallback) => Token(key) is double value ? value : fallback;
    private static double TokenPageMargin(string key, double fallback) => Token(key) is Thickness value ? value.Left : fallback;
    private static string TokenFontFamily(string key, string fallback) => Token(key) is FontFamily value ? value.Source : fallback;
}
