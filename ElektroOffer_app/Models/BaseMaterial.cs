namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 🧱 BaseMaterial – podklad práce, nese koeficient (tabulka "BaseMaterials")
    // =========================================================================
    //
    // Neplést se sekcí MATERIÁL (Models/Material.cs)! Tohle je čistě
    // koeficient pro výpočet ceny PRÁCE (Beton, Cihla, Sádra...).
    // Výběr není omezen podle WorkTask – nabízí se vždy celý seznam.
    // =========================================================================
    public class BaseMaterial
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BaseMaterialCoef { get; set; }
    }
}