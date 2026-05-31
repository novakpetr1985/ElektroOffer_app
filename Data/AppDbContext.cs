using Microsoft.EntityFrameworkCore;
using ElektroOffer_app.Models;

namespace ElektroOffer_app.Data
{
    // =========================
    // 🧠 DATABÁZOVÝ KONTEJNER (EF CORE)
    // =========================
    // 👉 Slouží jako most mezi C# objekty a SQLite databází
    // 👉 Nahrazuje ruční SQL (SqliteCommand, Reader)
    // =========================
    public class AppDbContext : DbContext
    {
        // =========================
        // 🔧 TABULKY V DATABÁZI
        // =========================
        public DbSet<PriceItems> PriceItems => Set<PriceItems>();
        public DbSet<Material> Materials => Set<Material>();

        // =========================
        // 🧩 KONFIGURACE DATABÁZE
        // =========================
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // ochrana proti dvojité konfiguraci
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=elektrooffer.db");
            }
        }
    }
}