using ElektroOffer_app.Models;

public class CalculationItems
{
    public PriceItems Item { get; set; }

    public double Quantity { get; set; }

    public double Total
    {
        get
        {
            if (Item == null)
                return 0;

            return Item.BasePrice *
                   Item.MaterialCoef *
                   Item.PositionCoef *
                   Quantity;
        }
    }
}