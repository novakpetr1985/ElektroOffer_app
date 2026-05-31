namespace ElektroOffer_app.Models
{
    // =========================
    // 📦 MATERIÁL Z DB
    // =========================
    public class Material
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public double Price { get; set; }

        public string Unit { get; set; } = string.Empty;
    }
}