﻿namespace ElektroOffer_app.Models
{
    /// <summary>
    /// Reprezentuje celý uložený projekt (kalkulaci) jako jeden celek.
    ///
    /// Tato třída se:
    /// - serializuje do JSON při uložení (Save)
    /// - deserializuje z JSON při načtení (Load)
    ///
    /// Obsahuje:
    /// - metadata projektu (název, datum vytvoření/uložení)
    /// - seznam řádků práce (WorkItems)
    /// - seznam řádků materiálu (MaterialItems)
    /// </summary>
    public class ProjectData
    {
        // ---------------------- METADATA PROJEKTU ----------------------

        /// <summary>
        /// Název projektu / zakázky.
        /// Zobrazuje se v titulku okna.
        /// </summary>
        public string ProjectName { get; set; } = "Nový projekt";

        /// <summary>
        /// Datum vytvoření projektu.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Datum posledního uložení.
        /// Aktualizuje se při Save.
        /// </summary>
        public DateTime SavedAt { get; set; } = DateTime.Now;

        // ---------------------- SEKCE: PRÁCE ----------------------

        /// <summary>
        /// Seznam všech řádků sekce PRÁCE.
        /// Každý řádek obsahuje vybraný úkon, upřesnění, materiál, umístění a množství.
        /// </summary>
        public List<WorkItemData> WorkItems { get; set; } = new();

        // ---------------------- SEKCE: MATERIÁL ----------------------

        /// <summary>
        /// Seznam všech řádků sekce MATERIÁL.
        /// Každý řádek obsahuje název materiálu a množství.
        /// </summary>
        public List<MaterialItemData> MaterialItems { get; set; } = new();
    }
}
