using System;
using System.Collections.Generic;
using System.Text;
namespace ElektroOffer_app.Models
{
    public class WorkItem
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal PricePerUnit { get; set; }

        public string Unit { get; set; } = "ks";
    }
}