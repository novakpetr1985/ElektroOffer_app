﻿namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 📦 MaterialItemData – data pro jeden řádek materiálu v projektu
    // =========================================================================
    //
    // K čemu slouží:
    // - Reprezentuje jeden řádek sekce MATERIÁL v uloženém projektu (ProjectData)
    // - Ukládá se do JSON při Save a načítá při Load
    //
    // Vlastnosti:
    // - MaterialName      → název materiálu (string, ne ID)
    // - Quantity          → množství
    // - IsDiscountEnabled → příznak aktivace slevy na tomto řádku
    // - DiscountPercent   → procentuální výše slevy (null = nezadána)
    //
    // Poznámka:
    // - Při načtení projektu se podle MaterialName dohledá konkrétní Material
    //   z kolekce Materials (načtené z databáze).
    // - IsDiscountEnabled výchozí false → staré .eof soubory bez slevy se načtou správně.
    // =========================================================================
    public class MaterialItemData
    {
        public string? MaterialName { get; set; }
        public double Quantity { get; set; }

        // ---------------------- SLEVA ----------------------

        /// <summary>
        /// Příznak aktivace slevy na tomto řádku.
        /// Výchozí false → staré .eof soubory bez slevy se načtou správně.
        /// </summary>
        public bool IsDiscountEnabled { get; set; }

        /// <summary>
        /// Procentuální výše slevy (0–100).
        /// Null = sleva není zadána.
        /// </summary>
        public double? DiscountPercent { get; set; }
    }
}