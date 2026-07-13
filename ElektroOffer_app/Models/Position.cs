namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 📍 Position – umístění/poloha provádění práce ovlivňující cenu koeficientem
    // =========================================================================
    //
    // Účel:
    // - Obdoba BaseMaterial, ale pro POLOHU práce (Nízká, Strop, Stěna, Štafle).
    // - Nabízí se VŽDY celý seznam, bez filtrování.
    // =========================================================================
    public class Position
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Koeficient, kterým se násobí BasePrice úkonu.
        public double PositionCoef { get; set; }
    }
}