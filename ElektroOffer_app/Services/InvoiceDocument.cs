using System;
using System.Collections.Generic;
using System.Linq;
using ElektroOffer_app.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ElektroOffer_app.Services
{
    // ============================================================================
    // 🧾 InvoiceDocument – vizuální layout faktury (QuestPDF)
    // ----------------------------------------------------------------------------
    // Účel:
    //   • Definuje, jak faktura vypadá na papíře/PDF – hlavička (dodavatel +
    //     odběratel + čísla/data), tabulka položek, součty, patička.
    //   • Neobsahuje ŽÁDNOU obchodní logiku (mapování z kalkulace) – to dělá
    //     InvoiceTemplateService. Tahle třída jen VYKRESLUJE hotová data.
    //
    // Podmíněné zobrazení DPH:
    //   • Pokud Supplier.IsVatPayer == false, tabulka má jen sloupce
    //     Popis / Množství / MJ / Cena za MJ / Celkem.
    //   • Pokud Supplier.IsVatPayer == true, přibývá sloupec Sazba DPH a
    //     patička obsahuje rozpis Základ / DPH / Celkem s DPH.
    //
    // Použití:
    //   var document = new InvoiceDocument(invoiceData, supplierSettings);
    //   document.GeneratePdf(cesta_k_souboru.pdf);
    // ============================================================================
    public class InvoiceDocument : IDocument
    {
        private readonly InvoiceItemData _invoice;
        private readonly SupplierSettings _supplier;

        public InvoiceDocument(InvoiceItemData invoice, SupplierSettings supplier)
        {
            _invoice = invoice ?? throw new ArgumentNullException(nameof(invoice));
            _supplier = supplier ?? throw new ArgumentNullException(nameof(supplier));
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Strana ");
                    x.CurrentPageNumber();
                    x.Span(" z ");
                    x.TotalPages();
                });
            });
        }

        // ------------------------------------------------------------------
        // 🏷 ComposeHeader – číslo faktury, dodavatel, odběratel, data
        // ------------------------------------------------------------------
        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"Faktura č. {_invoice.InvoiceNumber}")
                        .FontSize(18).Bold();
                });

                column.Item().PaddingTop(10).Row(row =>
                {
                    // Dodavatel (vlevo)
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Dodavatel").Bold();
                        col.Item().Text(_supplier.CompanyName);
                        col.Item().Text(_supplier.Address);
                        col.Item().Text($"IČO: {_supplier.Ico}");

                        if (_supplier.IsVatPayer && !string.IsNullOrWhiteSpace(_supplier.Dic))
                            col.Item().Text($"DIČ: {_supplier.Dic}");

                        col.Item().Text($"Bankovní účet: {_supplier.BankAccount}");
                    });

                    // Odběratel (vpravo)
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Odběratel").Bold();
                        col.Item().Text(_invoice.CustomerName);
                        col.Item().Text(_invoice.CustomerAddress);

                        if (!string.IsNullOrWhiteSpace(_invoice.CustomerIco))
                            col.Item().Text($"IČO: {_invoice.CustomerIco}");

                        if (!string.IsNullOrWhiteSpace(_invoice.CustomerDic))
                            col.Item().Text($"DIČ: {_invoice.CustomerDic}");
                    });
                });

                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Text($"Datum vystavení: {_invoice.IssueDate:d.M.yyyy}");
                    row.RelativeItem().Text($"Datum splatnosti: {_invoice.DueDate:d.M.yyyy}");
                });
            });
        }

        // ------------------------------------------------------------------
        // 📋 ComposeContent – tabulka položek + součty
        // ------------------------------------------------------------------
        private void ComposeContent(IContainer container)
        {
            container.PaddingTop(15).Column(column =>
            {
                column.Item().Element(ComposeTable);
                column.Item().PaddingTop(10).Element(ComposeTotals);

                if (!string.IsNullOrWhiteSpace(_invoice.Note))
                {
                    column.Item().PaddingTop(15).Text($"Poznámka: {_invoice.Note}");
                }
            });
        }

        // ------------------------------------------------------------------
        // 🧮 ComposeTable – tabulka řádků faktury
        // ------------------------------------------------------------------
        // Sloupce se liší podle Supplier.IsVatPayer (viz komentář u třídy).
        // ------------------------------------------------------------------
        private void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);   // Popis
                    columns.RelativeColumn(1);   // Množství
                    columns.RelativeColumn(1);   // MJ
                    columns.RelativeColumn(1.5f); // Cena za MJ

                    if (_supplier.IsVatPayer)
                        columns.RelativeColumn(1); // Sazba DPH

                    columns.RelativeColumn(1.5f); // Celkem
                });

                table.Header(header =>
                {
                    header.Cell().Text("Popis").Bold();
                    header.Cell().AlignRight().Text("Množství").Bold();
                    header.Cell().Text("MJ").Bold();
                    header.Cell().AlignRight().Text("Cena za MJ").Bold();

                    if (_supplier.IsVatPayer)
                        header.Cell().AlignRight().Text("DPH").Bold();

                    header.Cell().AlignRight().Text("Celkem").Bold();

                    header.Cell().ColumnSpan((uint)(_supplier.IsVatPayer ? 6 : 5))
                        .PaddingTop(3).BorderBottom(1);
                });

                foreach (var line in _invoice.Lines)
                {
                    table.Cell().Text(line.Description);
                    table.Cell().AlignRight().Text(line.Quantity.ToString("0.##"));
                    table.Cell().Text(line.Unit);
                    table.Cell().AlignRight().Text($"{line.UnitPrice:0.00} Kč");

                    if (_supplier.IsVatPayer)
                        table.Cell().AlignRight().Text($"{line.VatRate:0}%");

                    table.Cell().AlignRight().Text($"{line.LineTotal:0.00} Kč");
                }
            });
        }

        // ------------------------------------------------------------------
        // 💰 ComposeTotals – celkový součet (s rozpisem DPH, pokud plátce)
        // ------------------------------------------------------------------
        private void ComposeTotals(IContainer container)
        {
            var subtotal = _invoice.Lines.Sum(l => l.LineTotal);

            container.AlignRight().Column(column =>
            {
                if (!_supplier.IsVatPayer)
                {
                    column.Item().Text($"Celkem k úhradě: {subtotal:0.00} Kč").Bold().FontSize(13);
                    return;
                }

                // Rozpis DPH po sazbách (pro případ, že by řádky měly různé sazby)
                var vatGroups = _invoice.Lines
                    .GroupBy(l => l.VatRate ?? 0)
                    .Select(g => new
                    {
                        Rate = g.Key,
                        Base = g.Sum(l => l.LineTotal),
                        Vat = g.Sum(l => l.LineTotal * (decimal)(g.Key / 100.0))
                    })
                    .OrderBy(g => g.Rate);

                foreach (var group in vatGroups)
                {
                    column.Item().Text(
                        $"Základ ({group.Rate:0}%): {group.Base:0.00} Kč   " +
                        $"DPH: {group.Vat:0.00} Kč");
                }

                var totalVat = vatGroups.Sum(g => g.Vat);
                var totalWithVat = subtotal + totalVat;

                column.Item().PaddingTop(5)
                    .Text($"Celkem k úhradě: {totalWithVat:0.00} Kč").Bold().FontSize(13);
            });
        }
    }
}