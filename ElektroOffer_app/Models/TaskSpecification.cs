namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 🔗 TaskSpecification – validní pár WorkTask ↔ WorkSpecification
    // =========================================================================
    //
    // ÚČEL:
    // Jediné omezení v kaskádě PRÁCE. Po výběru WorkTask se nabídka
    // WorkSpecification zúží jen na páry uložené v této tabulce
    // (např. "Drážkování" jde jen se "Spára").
    //
    // BaseMaterial a WorkPosition NEJSOU touto tabulkou nijak omezeny –
    // nabízí se u každé kombinace vždy celý seznam.
    //
    // Cena se NEČTE odsud – vždy se počítá za běhu:
    //   WorkTask.BasePrice × BaseMaterial.BaseMaterialCoef × WorkPosition.PositionCoef × Quantity
    // =========================================================================
    public class TaskSpecification
    {
        public int Id { get; set; }

        public int TaskId { get; set; }
        public WorkTask? Task { get; set; }

        public int SpecificationId { get; set; }
        public WorkSpecification? Specification { get; set; }
    }
}