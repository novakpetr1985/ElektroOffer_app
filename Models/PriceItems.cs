namespace ElektroOffer_app.Models
{
    // =========================
    // 🔧 CENÍK PRÁCE / SLUŽEB
    // =========================
    // 👉 Reprezentuje položku práce (např. montáž zásuvky)
    // 👉 Obsahuje koeficienty pro výpočet ceny
    // =========================
    public class PriceItems
    {
        public int Id { get; set; }

        // základní cena práce
        public double BasePrice { get; set; }

        // jednotka (hodina, ks)
        public string Unit { get; set; } = string.Empty;

        // název práce
        public string Task { get; set; } = string.Empty;

        // specifikace práce
        public string Specification { get; set; } = string.Empty;

        // typ materiálu (popis)
        public string Material { get; set; } = string.Empty;

        // umístění (např. interiér / exteriér)
        public string Location { get; set; } = string.Empty;

        // koeficient materiálu
        public double MaterialCoef { get; set; }

        // koeficient polohy
        public double PositionCoef { get; set; }

        // =========================
        // 🧾 TEXT PRO UI (COMBOBOX)
        // =========================
        public string FullName =>
            $"{Task} | {Specification} | {Material} | {Location}";
    }
}