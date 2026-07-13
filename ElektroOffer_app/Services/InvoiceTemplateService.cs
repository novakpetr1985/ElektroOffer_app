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
        //   • Skládá novou kaskádu WorkTask → WorkSpecification → BaseMaterial → Position
        //     do jednoho čitelného textu.
        //   • Každý segment je objekt (ne string), proto se používá .Name.
        //   • Prázdné segmenty (null/whitespace) se automaticky vynechají.
        //
        // LineType:
        //   • Nastaveno na "Práce" pro odlišení v PDF / UI.
        // ------------------------------------------------------------------
        private InvoiceLineData BuildWorkLine(CalculationItemViewModel item)
        {
            // 1) Sestavení popisu řádku z nových objektů
            var descriptionParts = new List<string?>
    {
        item.SelectedWorkTask?.Name,          // Název úkonu (např. "Montáž zásuvky")
        item.SelectedWorkSpecification?.Name, // Upřesnění (např. "1x zásuvka")
        item.SelectedBaseMaterial?.Name,      // Podklad (např. "Sádrokarton")
        item.SelectedPosition?.Name           // Poloha (např. "Strop")
    }
            .Where(x => !string.IsNullOrWhiteSpace(x));

            // 2) Vytvoření řádku faktury
            return new InvoiceLineData
            {
                LineType = "Práce",

                // Např. "Montáž zásuvky – 1x zásuvka – Sádrokarton – Strop"
                Description = string.Join(" – ", descriptionParts),

                Quantity = item.Quantity,

                // Jednotka pochází ze Specification (např. "ks", "m", "hod")
                Unit = item.SelectedWorkSpecification?.Unit ?? string.Empty,

                // Jednotková cena = BasePrice × MaterialCoef × PositionCoef
                UnitPrice = item.CalculatedWorkPrice ?? 0m
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
        // Unit / UnitPrice:
        //   • Dříve: SelectedMaterialUnit / SelectedMaterialPriceValue.
        //   • Nově: MaterialUnit / MaterialPrice (moderní properties ViewModelu).
        //   • Hodnoty jsou přenášeny z SelectedMaterialPrice (MaterialPrice?).
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

                // 🔵 NOVÉ – jednotka materiálu (nahrazuje SelectedMaterialUnit)
                Unit = item.MaterialUnit ?? string.Empty,

                // 🔵 NOVÉ – cena materiálu (nahrazuje SelectedMaterialPriceValue)
                UnitPrice = item.MaterialPrice ?? 0m,

                // Sleva – společná logika
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
