using System;
using System.Collections.Generic;
using System.Text;

namespace ElektroOffer_app.Models
{
    public class PriceItems
    {
        public int Id { get; set; }
        public double BasePrice { get; set; }
        public string Unit { get; set; }
        public string Task { get; set; }
        public string Specification { get; set; }
        public string Material { get; set; }
        public string Location { get; set; }
        public double MaterialCoef { get; set; }
        public double PositionCoef { get; set; }

        public string FullName =>
            $"{Task} | {Specification} | {Material} | {Location}";
    }
}