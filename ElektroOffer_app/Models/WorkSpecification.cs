using System.Collections.Generic;

namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 📄 WorkSpecification – upřesnění úkonu (dřív "Specification" v PriceItems)
    // =========================================================================
    public class WorkSpecification
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;

        // Navigační vlastnost – povolené kombinace s Úkonem.
        public List<TaskSpecification> TaskSpecifications { get; set; } = new();
    }
}