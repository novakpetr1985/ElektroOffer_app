namespace ElektroOffer_app.Models
{
    // =========================================================
    // 🔧 DATA PRO JEDEN ŘÁDEK PRÁCE
    // =========================================================
    public class WorkItemData
    {
        public string? SelectedTask { get; set; }
        public string? SelectedSpecification { get; set; }
        public string? SelectedMaterial { get; set; }
        public string? SelectedLocation { get; set; }
        public double Quantity { get; set; }
    }
}
