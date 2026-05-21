using Microsoft.EntityFrameworkCore;
using ElektroOffer_app.Models;

namespace ElektroOffer_app.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<WorkItem> WorkItems => Set<WorkItem>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=elektrooffer.db");
        }
    }
}