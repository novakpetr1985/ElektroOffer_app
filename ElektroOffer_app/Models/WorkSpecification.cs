namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 📄 WorkSpecification – upřesnění úkonu (tabulka "Specifications")
    // =========================================================================
    //
    // Do výpočtu ceny nevstupuje – slouží pouze:
    //   1) k omezení nabídky podle vybraného WorkTask (viz TaskSpecification),
    //   2) k zobrazení jednotky (Unit) u vypočtené ceny (m, ks...).
    // =========================================================================
    public class WorkSpecification
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Unit { get; set; }
    }
}