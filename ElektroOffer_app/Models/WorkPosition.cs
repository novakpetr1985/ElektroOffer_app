namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 📍 WorkPosition – poloha provedení práce, nese koeficient (tabulka "Positions")
    // =========================================================================
    //
    // ⚠️ Pojmenováno WorkPosition (ne "Position"), aby nekolidovalo
    // s WorkItemData.Position (pozice řádku v UI – jiný význam!).
    // Výběr není omezen podle WorkTask – nabízí se vždy celý seznam.
    // =========================================================================
    public class WorkPosition
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal PositionCoef { get; set; }
    }
}