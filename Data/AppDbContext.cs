using Microsoft.EntityFrameworkCore;
using ElektroOffer_app.Models;

namespace ElektroOffer_app.Data
{
    // =========================
    // 🧠 EF CORE DATABÁZOVÝ KONTEXT
    // =========================
    // 👉 Řídí komunikaci mezi aplikací a SQLite
    // 👉 Nahrazuje ruční SQL dotazy
    // =========================
    public class AppDbContext : DbContext
    {
        // =========================
        // 🔧 TABULKA PRÁCE
        // =========================
        public DbSet<PriceItems> PriceItems => Set<PriceItems>();

        // =========================
        // 📦 TABULKA MATERIÁL
        // =========================
        public DbSet<Material> Materials => Set<Material>();

        // =========================
        // 🧩 KONFIGURACE DB
        // =========================
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=elektrooffer.db");
            }
        }
    }
}
