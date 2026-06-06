using ElektroOffer_app.Models;

namespace ElektroOffer_app.Models
{
    /// <summary>
    /// Reprezentuje celý uložený projekt (kalkulaci) jako jeden celek.
    /// Tato třída se serializuje do JSON při Save a deserializuje při Load.
    /// Obsahuje všechny sekce: práce, materiál a metadata projektu.
    /// </summary>
    public class ProjectData
    {
        // =========================================================
        // 📋 METADATA PROJEKTU
        // =========================================================

        /// <summary>Název projektu / zakázky (zobrazí se v titulku okna)</summary>
        public string ProjectName { get; set; } = "Nový projekt";

        /// <summary>Datum vytvoření projektu</summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>Datum posledního uložení</summary>
        public DateTime SavedAt { get; set; } = DateTime.Now;

        // =========================================================
        // 🔧 SEKCE: PRÁCE
        // =========================================================

        /// <summary>
        /// Seznam všech řádků sekce PRÁCE.
        /// Každý řádek obsahuje vybraný úkon, upřesnění, materiál, umístění a množství.
        /// </summary>
        public List<WorkItemData> WorkItems { get; set; } = new();

        // =========================================================
        // 📦 SEKCE: MATERIÁL
        // =========================================================

        /// <summary>
        /// Seznam všech řádků sekce MATERIÁL.
        /// Každý řádek obsahuje název materiálu a množství.
        /// </summary>
        public List<MaterialItemData> MaterialItems { get; set; } = new();
    }

    // =============================================================
    // 🔧 DATA PRO JEDEN ŘÁDEK PRÁCE
    // =============================================================

    /// <summary>
    /// Serializovatelná data jednoho řádku sekce PRÁCE.
    /// Ukládají se názvy (string), protože PriceItems pracuje s názvy.
    /// </summary>
    public class WorkItemData
    {
        /// <summary>Vybraný úkon (Task), např. "Drážkování"</summary>
        public string? SelectedTask { get; set; }

        /// <summary>Vybrané upřesnění (Specification), např. "Spára"</summary>
        public string? SelectedSpecification { get; set; }

        /// <summary>Vybraný materiál (Material), např. "Omítka - Sádra"</summary>
        public string? SelectedMaterial { get; set; }

        /// <summary>Vybrané umístění (Location), např. "Stěna"</summary>
        public string? SelectedLocation { get; set; }

        /// <summary>Zadané množství (Quantity)</summary>
        public double Quantity { get; set; }
    }

    // =============================================================
    // 📦 DATA PRO JEDEN ŘÁDEK MATERIÁLU
    // =============================================================

    /// <summary>
    /// Serializovatelná data jednoho řádku sekce MATERIÁL.
    /// Ukládá se název materiálu — ten slouží k opětovnému dohledání záznamu z DB.
    /// </summary>
    public class MaterialItemData
    {
        /// <summary>
        /// Název materiálu, který byl vybrán v ComboBoxu.
        /// Při načítání se dohledá odpovídající objekt Material z DB podle tohoto názvu.
        /// </summary>
        public string? MaterialName { get; set; }

        /// <summary>Zadané množství (Quantity)</summary>
        public double Quantity { get; set; }
    }
}
