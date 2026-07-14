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
            // ---------------------------------------------------------
            // Cena práce se skládá ze základní ceny úkonu a dvou koeficientů.
            // Všechny 3 pracovní entity musí být vybrané, jinak řádek nemá cenu.
            // ---------------------------------------------------------
            if (vm.SelectedWorkTaskEntity != null &&
                vm.SelectedBaseMaterialEntity != null &&
                vm.SelectedWorkPositionEntity != null)
            {
                return (double)vm.SelectedWorkTaskEntity.BasePrice
                     * (double)vm.SelectedBaseMaterialEntity.BaseMaterialCoef
                     * (double)vm.SelectedWorkPositionEntity.PositionCoef
                     * vm.Quantity;
            }

            // ---------------------------------------------------------
            // Výpočet PRODUKTOVÉHO materiálu (beze změny)
            //
            // Čte se vm.SelectedMaterialPrice.Price – cena z konkrétně
            // vybrané nabídky (Název + Dodavatel + Materiál), nastavená
            // MaterialCascadeService.UpdateSelectedPrice().
            //
            // "(double)" přetypování je nutné, protože MaterialPrice.Price
            // je typu "decimal" (kvůli přesnosti peněz), zatímco tahle
            // metoda a Quantity pracují s "double".
            // ---------------------------------------------------------
            if (vm.SelectedMaterialPrice != null)
            {
                return (double)vm.SelectedMaterialPrice.Price * vm.Quantity;
            }

            // Nic není vybráno
            return 0;
        }

        // =========================================================
        // APLIKACE SLEVY (beze změny)
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
