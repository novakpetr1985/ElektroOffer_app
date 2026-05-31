namespace ElektroOffer_app.Models
{
    // =========================
    // 🔧 CENÍK PRÁCE
    // =========================
    // 👉 Jeden řádek ceníku práce
    // 👉 Obsahuje koeficienty pro výpočet ceny
    // =========================
    public class PriceItems
    {
        public int Id { get; set; }

        // =========================
        // 💰 ZÁKLADNÍ CENA
        // =========================
        public double BasePrice { get; set; }

        // =========================
        // 📏 JEDNOTKA
        // =========================
        public string Unit { get; set; } = string.Empty;

        // =========================
        // 🔧 NÁZEV PRÁCE
        // =========================
        public string Task { get; set; } = string.Empty;

        // =========================
        // 📄 SPECIFIKACE
        // =========================
        public string Specification { get; set; } = string.Empty;

        // =========================
        // 📦 TYP MATERIÁLU
        // =========================
        public string Material { get; set; } = string.Empty;

        // =========================
        // 📍 UMÍSTĚNÍ
        // =========================
        public string Location { get; set; } = string.Empty;

        // =========================
        // 📊 KOEFICIENT MATERIÁLU
        // =========================
        public double MaterialCoef { get; set; }

        // =========================
        // 📊 KOEFICIENT POZICE
        // =========================
        public double PositionCoef { get; set; }

        // =========================
        // 🧾 TEXT PRO COMBOBOX
        // =========================
        public string FullName =>
            $"{Task} | {Specification} | {Material} | {Location}";
    }
}