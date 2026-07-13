namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 🔗 TaskSpecification – vazební (M:N) tabulka Úkon ↔ Upřesnění
    // =========================================================================
    //
    // Účel:
    // - Řeší, které kombinace WorkTask + WorkSpecification jsou PLATNÉ.
    // - Obdoba role, kterou u Materiálu hraje MaterialPrice mezi Material
    //   a Supplier – ale bez vlastních dat navíc (žádná cena tady není,
    //   ta se počítá z WorkTask.BasePrice × koeficienty).
    //
    // Unikátní index (TaskId, SpecificationId) je nastaven v
    // AppDbContext.OnModelCreating(), aby nemohla vzniknout duplicitní
    // kombinace při importu/seedu.
    // =========================================================================
    public class TaskSpecification
    {
        public int Id { get; set; }

        public int TaskId { get; set; }
        public WorkTask Task { get; set; } = null!;

        public int SpecificationId { get; set; }
        public WorkSpecification Specification { get; set; } = null!;
    }
}