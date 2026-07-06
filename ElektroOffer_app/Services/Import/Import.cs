using System;

namespace ElektroOffer_app.Services.DataImport
{
    // =========================================================================
    // 📄 Import – jeden řádek z CSV souboru Import_Master
    // =========================================================================
    //
    // K čemu slouží:
    // - Dočasná pomocná třída POUZE pro import dat z Excelu do databáze
    // - Neukládá se do databáze, neregistruje se v AppDbContext
    // - Odpovídá 1:1 sloupcům v Import_Master.xlsx / import_master.csv:
    //   Nazev, Kategorie, ELKOV_MJ, ELKOV_Kod, ELKOV_Nazev, ELKOV_Cena,
    //   ELKOV_Měna, EMAS_MJ, EMAS_Kod, EMAS_Nazev, EMAS_Cena, EMAS_Měna
    // - Slouží jako "mezisklad" dat mezi čtením CSV souboru (ImportCsvReader)
    //   a jejich zápisem do databáze (MaterialImportService)
    // =========================================================================
    public class Import
    {
        public string Nazev { get; set; } = "";
        public string Kategorie { get; set; } = "";

        public string ElkovMJ { get; set; } = "";
        public string ElkovKod { get; set; } = "";
        public string ElkovNazev { get; set; } = "";
        public decimal ElkovCena { get; set; }
        public string ElkovMena { get; set; } = "";

        public string EmasMJ { get; set; } = "";
        public string EmasKod { get; set; } = "";
        public string EmasNazev { get; set; } = "";
        public decimal EmasCena { get; set; }
        public string EmasMena { get; set; } = "";
    }
}