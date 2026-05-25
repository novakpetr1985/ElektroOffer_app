using System;
using System.Collections.Generic;
using System.Text;

namespace ElektroOffer_app.Models
{
    public class CalculationItems
    {
        public PriceItems Item { get; set; }

        public double Quantity { get; set; }

        public double Total =>
            Item.BasePrice * Item.MaterialCoef * Item.PositionCoef * Quantity;
    }
}