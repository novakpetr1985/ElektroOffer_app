namespace ElektroOffer_app.Models
{
    // =========================
    // 📦 MATERIÁL (TABULKA Materials)
    // =========================
    // 👉 Reprezentuje jeden řádek v databázi
    // 👉 Používá se pro výpočet ceny materiálu
    // =========================
    public class Material
    {
        public int Id { get; set; }

        // název materiálu (např. kabel, jistič)
        public string Name { get; set; } = string.Empty;

        // cena za jednotku
        public double Price { get; set; }

        // jednotka (m, ks, balení)
        public string Unit { get; set; } = string.Empty;
    }
}