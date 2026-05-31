namespace ElektroOffer_app.Models
{
    // =========================
    // 🔧 CENÍK PRÁCE / POLOŽEK
    // =========================
    public class PriceItems
    {
        public int Id { get; set; }

        public double BasePrice { get; set; }

        public string Unit { get; set; } = string.Empty;

        public string Task { get; set; } = string.Empty;

        public string Specification { get; set; } = string.Empty;

        public string Material { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public double MaterialCoef { get; set; }

        public double PositionCoef { get; set; }

        // =========================
        // UI LABEL
        // =========================
        public string FullName =>
            $"{Task} | {Specification} | {Material} | {Location}";
    }
}