using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;

namespace ElektroOffer_app.Services
{
    // ============================================================================
    // 🧮 CalculationPriceService – výpočet celkové ceny řádku kalkulace
    // ----------------------------------------------------------------------------
    // Účel:
    //   • Centralizuje výpočet ceny řádku (Práce i Materiál).
    //   • Nahrazuje původní logiku v CalculationItemViewModel.Total.
    //   • Zajišťuje jednotné chování při aplikaci slevy.
    //
    // Podporované kaskády:
    //   • PRÁCE – WorkTask → WorkSpecification → BaseMaterial → Position
    //   • MATERIÁL – Category → ProductName → Supplier → Offer → Price
    //
    // Poznámka:
    //   • Výpočet práce používá BasePrice × MaterialCoef × PositionCoef.
    //   • Výpočet materiálu používá SelectedMaterialPrice.Price.
    //   • Sleva se aplikuje až po výpočtu základní ceny.
    // ============================================================================
    public class CalculationPriceService
    {
        // ----------------------------------------------------------------------
        // TOTAL – vrací celkovou cenu řádku včetně slevy
        // ----------------------------------------------------------------------
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
        //
        // Účel:
        //   • Vrací základní cenu řádku kalkulace (bez slevy).
        //   • Podporuje dvě zcela odlišné kaskády:
        //       1) PRÁCE – nová normalizovaná kaskáda WorkTask → BaseMaterial → Position
        //       2) MATERIÁL – produktová kaskáda Category → ProductName → Supplier → Offer → Price
        //
        // Logika:
        //   • Pokud je vyplněná kaskáda Práce (WorkTask + BaseMaterial + Position),
        //     počítá se cena práce.
        //   • Pokud je vyplněná kaskáda Materiálu (SelectedMaterialPrice),
        //     počítá se cena materiálu.
        //   • Pokud není vyplněno nic, vrací 0.
        //
        // Poznámka:
        //   • Quantity je typu double, BasePrice/Price jsou decimal → nutné přetypování.
        //   • WorkSpecification neovlivňuje cenu — určuje pouze jednotku (Unit).
        // =========================================================
        private double CalculateBaseTotal(CalculationItemViewModel vm)
        {
            // ---------------------------------------------------------
            // 🔴 PRÁCE – nová kaskáda WorkTask → BaseMaterial → Position
            //
            // Podmínky:
            //   • Musí být vybrán WorkTask (obsahuje BasePrice)
            //   • Musí být vybrán BaseMaterial (obsahuje MaterialCoef)
            //   • Musí být vybrána Position (obsahuje PositionCoef)
            //
            // Výpočet:
            //   Jednotková cena práce =
            //       BasePrice × MaterialCoef × PositionCoef
            //
            //   Celková cena práce =
            //       Jednotková cena × Quantity
            //
            // Poznámka:
            //   • WorkSpecification neovlivňuje cenu — určuje pouze jednotku (Unit).
            // ---------------------------------------------------------
            if (vm.SelectedWorkTask != null &&
                vm.SelectedBaseMaterial != null &&
                vm.SelectedPosition != null)
            {
                double unitPrice =
                    (double)vm.SelectedWorkTask.BasePrice *
                    vm.SelectedBaseMaterial.MaterialCoef *
                    vm.SelectedPosition.PositionCoef;

                return unitPrice * vm.Quantity;
            }

            // ---------------------------------------------------------
            // 🔵 MATERIÁL – produktová kaskáda (beze změny)
            //
            // Podmínky:
            //   • Musí být vybrána konkrétní nabídka (SelectedMaterialPrice)
            //     – obsahuje cenu konkrétního dodavatele.
            //
            // Výpočet:
            //   Celková cena materiálu =
            //       SelectedMaterialPrice.Price × Quantity
            //
            // Poznámka:
            //   • Price je decimal → nutné přetypování na double.
            // ---------------------------------------------------------
            if (vm.SelectedMaterialPrice != null)
            {
                return (double)vm.SelectedMaterialPrice.Price * vm.Quantity;
            }

            // ---------------------------------------------------------
            // 🔘 NIC NENÍ VYBRÁNO
            // ---------------------------------------------------------
            return 0;
        }

        // =========================================================
        // APLIKACE SLEVY
        // =========================================================
        //
        // Účel:
        //   • Aplikuje procentuální slevu na základní cenu.
        //
        // Logika:
        //   • Pokud sleva není zapnutá nebo není vyplněná, vrací se původní cena.
        //   • Sleva >= 100 % → cena nesmí být záporná → vrací 0.
        //   • Sleva < 0 → ignoruje se.
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
