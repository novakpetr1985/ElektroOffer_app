﻿namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 📦 Material – jedna položka ceníku materiálu
    // =========================================================================
    //
    // Vlastnosti:
    // - Id    → primární klíč v databázi
    // - Name  → název materiálu (např. "Kabel CYKY 3x1,5")
    // - Unit  → měrná jednotka (m, ks, balení, …)
    //
    // 🔴 ZMĚNA – Price odstraněno:
    // - Cena se ukládá výhradně v MaterialPrice (vázaná na dodavatele).
    // =========================================================================
    public partial class Material
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;
    }
}