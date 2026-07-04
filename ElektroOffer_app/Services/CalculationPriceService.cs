using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;

namespace ElektroOffer_app.Services
{
    /// <summary>
    /// Služba pro výpočet celkové ceny řádku kalkulace.
    /// Obsahuje veškerou logiku, která byla dříve v CalculationItemViewModel.Total.
    /// </summary>
    public class CalculationPriceService
    {
        /// <summary>
        /// Vrátí celkovou cenu řádku (práce nebo materiál) včetně slevy.
        /// </summary>
        public double CalculateTotal(CalculationItemViewModel vm)
        {
            double baseTotal = CalculateBaseTotal(vm);

            if (baseTotal <= 0)
                return 0;

            return ApplyDiscount(vm, baseTotal);
        }

        // =========================================================
        // ZÁKLADNÍ CENA (BEZ SLEVY)
        // =========================================================
        private double CalculateBaseTotal(CalculationItemViewModel vm)
        {
            // Výpočet práce
            if (vm.WorkItem != null)
            {
                return vm.WorkItem.BasePrice
                     * vm.WorkItem.MaterialCoef
                     * vm.WorkItem.PositionCoef
                     * vm.Quantity;
            }

            // Výpočet materiálu
            if (vm.MaterialItem != null)
            {
                return vm.MaterialItem.Price * vm.Quantity;
            }

            // Nic není vybráno
            return 0;
        }

        // =========================================================
        // APLIKACE SLEVY
        // =========================================================
        private double ApplyDiscount(CalculationItemViewModel vm, double baseTotal)
        {
            if (!vm.IsDiscountEnabled || !vm.DiscountPercent.HasValue)
                return baseTotal;

            double percent = vm.DiscountPercent.Value;

            // Sleva >= 100 % → cena nesmí být záporná
            if (percent >= 100)
                return 0;

            // Sleva < 0 → ignorovat
            if (percent < 0)
                return baseTotal;

            return baseTotal * (1 - percent / 100.0);
        }
    }
}
