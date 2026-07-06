﻿namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 📦 Material – jedna položka ceníku materiálu
    // =========================================================================
    //
    // K čemu slouží:
    // - Reprezentuje jeden záznam v tabulce Materials (SQLite)
    // - Používá se v AppDbContext (DbSet<Material>)
    // - V UI se používá např. pro výběr materiálu v kalkulaci
    //
    // Vlastnosti:
    // - Id    → primární klíč v databázi
    // - Name  → název materiálu (např. "Kabel CYKY 3x1,5")
    // - Price → cena za jednotku
    // - Unit  → měrná jednotka (m, ks, balení, …)
    // =========================================================================
    public partial class Material
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public double Price { get; set; }

        public string Unit { get; set; } = string.Empty;
    }
}
