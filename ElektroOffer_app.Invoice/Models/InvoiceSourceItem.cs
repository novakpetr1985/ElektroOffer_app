namespace ElektroOffer_app.Invoice.Models
{
    /// <summary>Přenáší potvrzený řádek rozpočtu z hlavní aplikace do návrhu faktury.</summary>
    public class InvoiceSourceItem
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public double PriceBeforeDiscount { get; set; }
        public double Price { get; set; }
        public double? DiscountPercent { get; set; }
        public double? DiscountAmount { get; set; }
    }
}
