namespace ElektroOffer_app.Models
{
    /// <summary>
    /// Obálka pro export/import ceníku do JSON souboru.
    /// Obsahuje obě tabulky — PriceItems i Materials — v jednom souboru.
    /// Soubor má příponu .eofcat (ElektroOffer Catalog).
    /// </summary>
    public class CatalogExportData
    {
        /// <summary>Datum a čas exportu — pro orientaci při importu</summary>
        public DateTime ExportedAt { get; set; } = DateTime.Now;

        /// <summary>Verze formátu exportu — pro budoucí kompatibilitu</summary>
        public string FormatVersion { get; set; } = "1.0";

        // =========================================================
        // 🔧 CENÍK PRÁCE
        // =========================================================

        /// <summary>
        /// Seznam všech položek ceníku práce (tabulka PriceItems).
        /// Ukládají se názvy — nezávislé na ID v DB.
        /// </summary>
        public List<PriceItemExportData> PriceItems { get; set; } = new();

        // =========================================================
        // 📦 CENÍK MATERIÁLU
        // =========================================================

        /// <summary>
        /// Seznam všech položek ceníku materiálu (tabulka Materials).
        /// Ukládají se názvy — nezávislé na ID v DB.
        /// </summary>
        public List<MaterialExportData> Materials { get; set; } = new();
    }

    // =============================================================
    // 🔧 JEDNA POLOŽKA CENÍKU PRÁCE
    // =============================================================

    /// <summary>
    /// Serializovatelná data jedné položky z tabulky PriceItems.
    /// Odpovídá sloupcům: Task, Specification, Material, Location,
    /// BasePrice, Unit, MaterialCoef, PositionCoef.
    /// </summary>
    public class PriceItemExportData
    {
        /// <summary>Název úkonu, např. "Drážkování"</summary>
        public string Task { get; set; } = "";

        /// <summary>Upřesnění, např. "Spára"</summary>
        public string Specification { get; set; } = "";

        /// <summary>Název materiálu, např. "Omítka - Sádra"</summary>
        public string Material { get; set; } = "";

        /// <summary>Umístění, např. "Stěna"</summary>
        public string Location { get; set; } = "";

        /// <summary>Základní cena bez koeficientů</summary>
        public double BasePrice { get; set; }

        /// <summary>Měrná jednotka, např. "m", "ks", "hod"</summary>
        public string Unit { get; set; } = "";

        /// <summary>Koeficient materiálu (násobí základní cenu)</summary>
        public double MaterialCoef { get; set; }

        /// <summary>Koeficient polohy/umístění (násobí základní cenu)</summary>
        public double PositionCoef { get; set; }
    }

    // =============================================================
    // 📦 JEDNA POLOŽKA CENÍKU MATERIÁLU
    // =============================================================

    /// <summary>
    /// Serializovatelná data jedné položky z tabulky Materials.
    /// Odpovídá sloupcům: Name, Unit, Price (a případně dalším).
    /// </summary>
    public class MaterialExportData
    {
        /// <summary>Název materiálu, např. "Kabel CYKY 3x1,5"</summary>
        public string Name { get; set; } = "";

        /// <summary>Měrná jednotka, např. "m", "ks", "balení"</summary>
        public string Unit { get; set; } = "";

        /// <summary>Cena za jednotku</summary>
        public double Price { get; set; }
    }
}
