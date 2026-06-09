﻿using Microsoft.EntityFrameworkCore;
using ElektroOffer_app.Models;

namespace ElektroOffer_app.Data
{
    // =========================================================================
    // 🧠 AppDbContext – EF Core databázový kontext
    // =========================================================================
    //
    // K čemu slouží:
    // - Zprostředkovává komunikaci mezi aplikací a SQLite databází
    // - Nahrazuje ruční psaní SQL dotazů
    //
    // Jaké tabulky (DbSet) obsahuje:
    // - PriceItems  → ceník práce
    // - Materials   → ceník materiálu
    //
    // Kde se používá:
    // - V MainWindow (načítání ceníku do kolekcí Tasks a Materials)
    // - Při importu/exportu ceníku (zápis/čtení do/z DB)
    //
    // Poznámka:
    // - Konfigurace připojení je v OnConfiguring → SQLite soubor elektrooffer.db
    // =========================================================================
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Tabulka ceníku práce (PriceItems).
        /// Každý záznam reprezentuje jednu položku ceníku.
        /// </summary>
        public DbSet<PriceItems> PriceItems => Set<PriceItems>();

        /// <summary>
        /// Tabulka ceníku materiálu (Materials).
        /// Každý záznam reprezentuje jeden materiál.
        /// </summary>
        public DbSet<Material> Materials => Set<Material>();

        /// <summary>
        /// Konfigurace databázového připojení.
        /// Pokud ještě není kontext nakonfigurován, použije SQLite soubor
        /// "elektrooffer.db" v aktuálním adresáři aplikace.
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // TODO: případně přesunout connection string do konfigurace (appsettings.json)
                optionsBuilder.UseSqlite("Data Source=elektrooffer.db");
            }
        }
    }
}
