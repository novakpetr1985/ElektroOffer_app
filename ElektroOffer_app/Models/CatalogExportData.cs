﻿namespace ElektroOffer_app.Models
{
    /// <summary>
    /// Obálka pro export/import ceníku do JSON souboru.
    /// 
    /// Obsahuje obě tabulky:
    /// - PriceItems  → ceník práce
    /// - Materials   → ceník materiálu
    ///
    /// Soubor má příponu .eofcat (ElektroOffer Catalog).
    /// </summary>
    public class CatalogExportData
    {
        /// <summary>
        /// Datum a čas exportu – pro orientaci při importu (např. který ceník je novější).
        /// </summary>
        public DateTime ExportedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Verze formátu exportu – pro budoucí kompatibilitu.
        /// Pokud se struktura někdy změní, můžeš zvýšit číslo a podle toho reagovat při importu.
        /// </summary>
        public string FormatVersion { get; set; } = "1.0";

        /// <summary>
        /// Seznam všech položek ceníku práce (tabulka PriceItems).
        /// Ukládají se názvy – nezávislé na ID v databázi.
        /// </summary>
        public List<PriceItemExportData> PriceItems { get; set; } = new();

        /// <summary>
        /// Seznam všech položek ceníku materiálu (tabulka Materials).
        /// Ukládají se názvy – nezávislé na ID v databázi.
        /// </summary>
        public List<MaterialExportData> Materials { get; set; } = new();
    }

    /// <summary>
    /// Serializovatelná data jedné položky z tabulky PriceItems.
    /// Odpovídá sloupcům:
    /// - Task
    /// - Specification
    /// - Material
    /// - Location
    /// - BasePrice
    /// - Unit
    /// - MaterialCoef
    /// - PositionCoef
    /// </summary>
    public class PriceItemExportData
    {
        public string Task { get; set; } = "";
        public string Specification { get; set; } = "";
        public string Material { get; set; } = "";
        public string Location { get; set; } = "";
        public double BasePrice { get; set; }
        public string Unit { get; set; } = "";
        public double MaterialCoef { get; set; }
        public double PositionCoef { get; set; }
    }

    /// <summary>
    /// Serializovatelná data jedné položky z tabulky Materials.
    /// Odpovídá sloupcům:
    /// - Name
    /// - Unit
    /// - Price
    /// (případně dalším, pokud je v budoucnu přidáš).
    /// </summary>
    public class MaterialExportData
    {
        public string Name { get; set; } = "";
        public string Unit { get; set; } = "";
        public double Price { get; set; }
    }
}
