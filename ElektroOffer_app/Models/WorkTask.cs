using System.Collections.Generic;

namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 🛠 WorkTask – úkon práce (dřív "Task" v ceníku PriceItems)
    // =========================================================================
    //
    // Pojmenováno WorkTask (ne "Task"), aby nekolidovalo se System.Threading.
    // Tasks.Task. V databázi zůstává tabulka "Tasks" (viz OnModelCreating
    // v AppDbContext, pokud bys chtěl explicitně zachovat název tabulky).
    //
    // Platné kombinace s WorkSpecification řeší vazební tabulka
    // TaskSpecification (M:N) – viz komentář tamtéž.
    // =========================================================================
    public class WorkTask
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Základní cena úkonu, násobí se koeficienty BaseMaterial a Position.
        public decimal BasePrice { get; set; }

        // Navigační vlastnost – povolené kombinace se Specifikací.
        public List<TaskSpecification> TaskSpecifications { get; set; } = new();
    }
}