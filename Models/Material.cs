namespace ElektroOffer_app.Models
{
    // =========================
    // 📦 MATERIÁL
    // =========================
    // 👉 Jeden záznam materiálu v databázi
    // 👉 Používá se pro kalkulaci ceny materiálu
    // =========================
    public class Material
    {
        public int Id { get; set; }

        // =========================
        // 📦 NÁZEV MATERIÁLU
        // =========================
        public string Name { get; set; } = string.Empty;

        // =========================
        // 💰 CENA ZA JEDNOTKU
        // =========================
        public double Price { get; set; }

        // =========================
        // 📏 JEDNOTKA
        // =========================
        public string Unit { get; set; } = string.Empty;
    }
}