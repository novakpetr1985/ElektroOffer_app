using System.Collections.Generic;

namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 🧱 BaseMaterial – podkladový materiál ovlivňující cenu práce koeficientem
    // =========================================================================
    //
    // Účel:
    // - Na rozdíl od produktového Materiálu (Models/Material.cs) tahle entita
    //   NEREPREZENTUJE nakupovaný materiál, ale PODKLAD, do/na kterého se
    //   práce provádí (Beton, Cihla, Sádrokarton...).
    // - Slouží výhradně jako násobitel ceny práce (MaterialCoef).
    // - Nabízí se VŽDY celý seznam, bez filtrování podle Task/Specification.
    // =========================================================================
    public class BaseMaterial
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Koeficient, kterým se násobí BasePrice úkonu.
        public double MaterialCoef { get; set; }
    }
}