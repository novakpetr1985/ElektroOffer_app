using System;
using System.Collections.Generic;
using System.Linq;
using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;
using QuestPDF.Fluent;

namespace ElektroOffer_app.Services
{
    // ============================================================================
    // 🧾 InvoiceTemplateService – sestavení a generování faktury
    // ----------------------------------------------------------------------------
    // Účel:
    //   • Mapuje aktuální kalkulaci (WorkCalcItems + MaterialItems) na řádky
    //     faktury (InvoiceLineData).
    //   • Zajišťuje generování výsledného PDF přes QuestPDF (InvoiceDocument).
    //
    // Mapování řádků (1:1):
    //   • Každá NEPRÁZDNÁ položka kalkulace (Práce i Materiál) se stane jedním
    //     řádkem faktury. Prázdné řádky (IsEmpty == true) se stejně jako při
    //     ukládání projektu ignorují – viz CalculationItemViewModel.IsEmpty.
    //   • Popis řádku (Description) se skládá z kaskádových polí, aby byl
    //     na faktuře čitelný i bez znalosti vnitřní struktury appky.
    //
    // Poznámka k budoucímu rozšíření:
    //   • Pokud by se v budoucnu chtělo řádky seskupovat (např. všechen
    //     materiál do jednoho souhrnného řádku), stačí upravit pouze metody
    //     BuildWorkLine/BuildMaterialLine + volající LINQ dotazy níže –
    //     InvoiceLineData ani ostatní modely se měnit nemusí.
    // ============================================================================
    public class InvoiceTemplateService
    {
        // ------------------------------------------------------------------
        // 📋 BuildInvoiceLines – sestaví kompletní seznam řádků faktury
        // ------------------------------------------------------------------
        // Vstup:
        //   • workItems – kolekce pracovních položek z MainViewModel.
        //   • materialItems – kolekce materiálových položek z MainViewModel.
        //
        // Logika:
        //   • Filtruje prázdné řádky (IsEmpty == true).
        //   • Každou ne-prázdnou položku mapuje na InvoiceLineData
        //     pomocí BuildWorkLine / BuildMaterialLine.
        //
        // Výstup:
        //   • List<InvoiceLineData> připravený k uložení do InvoiceItemData.Lines
        //     a k vykreslení v InvoiceDocument.
        // ------------------------------------------------------------------
        public List<InvoiceLineData> BuildInvoiceLines(
            IEnumerable<CalculationItemViewModel> workItems,
            IEnumerable<CalculationItemViewModel> materialItems)
        {
            var lines = new List<InvoiceLineData>();

            // Pracovní položky → řádky typu "Práce".
            lines.AddRange(
                workItems
                    .Where(x => !x.IsEmpty)
                    .Select(BuildWorkLine));

            // Materiálové položky → řádky typu "Materiál".
            lines.AddRange(
                materialItems
                    .Where(x => !x.IsEmpty)
                    .Select(BuildMaterialLine));

            return lines;
        }

        // ------------------------------------------------------------------
        // 🔧 BuildWorkLine – jedna položka Práce → InvoiceLineData
        // ------------------------------------------------------------------
        // Description:
        //   • Skládá kaskádu Task → Specification → Material → Location
        //     do jednoho čitelného textu.
        //   • Prázdné segmenty (null/whitespace) se automaticky vynechají.
        //
        // LineType:
        //   • Můžeš doplnit např. "Práce", pokud chceš typ řádku odlišit
        //     i u pracovních položek (zatím není nastaven).
        // ------------------------------------------------------------------
        private InvoiceLineData BuildWorkLine(CalculationItemViewModel item)
        {
            var descriptionParts = new List<string?>
            {
                item.SelectedTask,
                item.SelectedSpecification,
                item.SelectedMaterial,
                item.SelectedLocation
            }
            .Where(part => !string.IsNullOrWhiteSpace(part));

            return new InvoiceLineData
            {
                // Např. "Montáž zásuvky – 1x zásuvka – Obývací pokoj"
                Description = string.Join(" – ", descriptionParts),
                Quantity = item.Quantity,
                Unit = item.SelectedWorkUnit ?? string.Empty,
                UnitPrice = item.SelectedWorkPrice ?? 0m
                // LineType můžeš doplnit později, pokud budeš chtít:
                // LineType = "Práce"
            };
        }

        // ------------------------------------------------------------------
        // 📦 BuildMaterialLine – jedna položka Materiálu → InvoiceLineData
        // ------------------------------------------------------------------
        // Description:
        //   • Pokud je vyplněn dodavatel (SelectedSupplier), zobrazí se
        //     ve formátu "ProductName (Supplier)".
        //   • Jinak se použije jen ProductName.
        //
        // LineType:
        //   • Nastaveno na "Materiál" – umožňuje v UI/PDF odlišit materiálové
        //     řádky od pracovních.
        //
        // DiscountPercent:
        //   • Pokud je v kalkulaci zapnutá sleva (IsDiscountEnabled),
        //     přenese se hodnota DiscountPercent.
        //   • Pokud sleva není zapnutá, je null.
        // ------------------------------------------------------------------
        private InvoiceLineData BuildMaterialLine(CalculationItemViewModel item)
        {
            var description = string.IsNullOrWhiteSpace(item.SelectedSupplier)
                ? item.SelectedProductName ?? string.Empty
                : $"{item.SelectedProductName} ({item.SelectedSupplier})";

            return new InvoiceLineData
            {
                LineType = "Materiál",
                Description = description,
                Quantity = item.Quantity,
                Unit = item.SelectedMaterialUnit ?? string.Empty,
                UnitPrice = item.SelectedMaterialPriceValue ?? 0m,
                DiscountPercent = item.IsDiscountEnabled ? item.DiscountPercent : null
            };
        }

        // ------------------------------------------------------------------
        // 🖨 GeneratePdf – vygeneruje a uloží PDF fakturu na disk
        // ------------------------------------------------------------------
        // Vstup:
        //   • invoiceData – fakturační údaje projektu (číslo, data, odběratel,
        //     Lines už musí být naplněné přes BuildInvoiceLines).
        //   • supplier – trvalé údaje dodavatele (z SupplierSettings).
        //   • outputPath – cílová cesta k PDF souboru (absolutní).
        //
        // Logika:
        //   • Vytvoří instanci InvoiceDocument (QuestPDF).
        //   • Vygeneruje PDF na danou cestu.
        // ------------------------------------------------------------------
        public void GeneratePdf(InvoiceItemData invoiceData, SupplierSettings supplier, string outputPath)
        {
            var document = new InvoiceDocument(invoiceData, supplier);
            document.GeneratePdf(outputPath);
        }
    }
}
