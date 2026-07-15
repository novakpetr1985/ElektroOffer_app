﻿namespace ElektroOffer_app.Models
{
    /// <summary>
    /// Obálka pro export/import ceníku do JSON souboru.
    /// 
    /// Obsahuje pracovní a materiálový ceník.
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
        /// Seznam všech položek pracovního ceníku.
        /// Ukládají se názvy – nezávislé na ID v databázi.
        /// </summary>
        public List<WorkCatalogExportData> WorkItems { get; set; } = new();

        /// <summary>
        /// Seznam všech položek ceníku materiálu (tabulka Materials).
        /// Ukládají se názvy – nezávislé na ID v databázi.
        /// </summary>
        public List<MaterialExportData> Materials { get; set; } = new();
    }

    /// <summary>
    /// Serializovatelná data jedné položky pracovního ceníku.
    /// </summary>
    public class WorkCatalogExportData
    {
        public string WorkTask { get; set; } = "";
        public string WorkSpecification { get; set; } = "";
        public string BaseMaterial { get; set; } = "";
        public string WorkPosition { get; set; } = "";
        public decimal BasePrice { get; set; }
        public string Unit { get; set; } = "";
        public decimal BaseMaterialCoef { get; set; }
        public decimal PositionCoef { get; set; }
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
