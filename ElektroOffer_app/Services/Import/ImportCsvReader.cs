
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ElektroOffer_app.Services.DataImport
{
    // =========================================================================
    // 📥 ImportCsvReader – načtení Import_Master.csv do seznamu Import
    // =========================================================================
    //
    // K čemu slouží:
    // - Přečte CSV soubor exportovaný z Excel listu Import_Master
    // - Převede každý textový řádek na jeden objekt Import
    // - Používá kulturu "cs-CZ" při parsování cen, protože Excel ukládá
    //   desetinná čísla s ČÁRKOU (23,22), ne s tečkou (23.22) - bez tohoto
    //   nastavení by decimal.Parse() skončil chybou FormatException
    //
    // Kódování souboru:
    // - Čte se přes StreamReader s detectEncodingFromByteOrderMarks: true,
    //   NE přes File.ReadAllLines(cesta, Encoding.UTF8). Důvod: CSV export
    //   z Excelu jako "UTF-8" obsahuje na začátku souboru neviditelný
    //   BOM (byte order mark) - pokud by se nezpracoval správně, mohl by
    //   se přilepit na první sloupec hlavičky a nenápadně rozbít pozdější
    //   porovnávání textů. StreamReader s touto volbou BOM sám rozpozná
    //   a odstraní, ať už v souboru je, nebo není.
    //
    // Omezení:
    // - Jednoduché rozdělení řádku podle středníku (Split(';')) - funguje
    //   spolehlivě, POKUD žádný text (např. SupplierName) neobsahuje
    //   středník. U aktuálních 10 položek to neplatí, ale při rozšiřování
    //   na desítky/stovky položek zvaž knihovnu CsvHelper (NuGet), která
    //   umí i "escapované" hodnoty se středníkem uvnitř uvozovek
    // =========================================================================
    public static class ImportCsvReader
    {
        public static List<Import> NactiImportCsv(string cesta)
        {
            var vysledek = new List<Import>();

            // StreamReader s automatickou detekcí BOM - bezpečnější
            // než File.ReadAllLines(cesta, Encoding.UTF8)
            using var reader = new StreamReader(cesta, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var obsahSouboru = reader.ReadToEnd();
            var radky = obsahSouboru.Split(
                new[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries);

            // Kultura "cs-CZ" zajišťuje správné parsování čísel s čárkou
            var czechCulture = new CultureInfo("cs-CZ");

            // Začínáme od indexu 1, protože radky[0] je hlavička sloupců
            for (int i = 1; i < radky.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(radky[i]))
                    continue; // přeskočíme prázdné řádky

                var sloupce = radky[i].Split(';');

                vysledek.Add(new Import
                {
                    Nazev = sloupce[0].Trim(),
                    Kategorie = sloupce[1].Trim(),

                    ElkovMJ = sloupce[2].Trim(),
                    ElkovKod = sloupce[3].Trim(),
                    ElkovNazev = sloupce[4].Trim(),
                    ElkovCena = decimal.Parse(sloupce[5].Trim(), czechCulture),
                    ElkovMena = sloupce[6].Trim(),

                    EmasMJ = sloupce[7].Trim(),
                    EmasKod = sloupce[8].Trim(),
                    EmasNazev = sloupce[9].Trim(),
                    EmasCena = decimal.Parse(sloupce[10].Trim(), czechCulture),
                    EmasMena = sloupce[11].Trim()
                });
            }

            return vysledek;
        }
    }
}