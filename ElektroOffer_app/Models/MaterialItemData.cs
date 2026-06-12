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
    // - MaterialName → název materiálu (string, ne ID)
    // - Quantity     → množství
    //
    // Poznámka:
    // - Při načtení projektu se podle MaterialName dohledá konkrétní Material
    //   z kolekce Materials (načtené z databáze).
    // =========================================================================
    public class MaterialItemData
    {
        public string? MaterialName { get; set; }
        public double Quantity { get; set; }
    }
}
