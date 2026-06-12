using ElektroOffer_app.Models;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.LogicTests
{
    /// <summary>
    /// Testy výpočtů cen pro model PriceItems.
    /// Testují kombinace BasePrice, MaterialCoef a PositionCoef.
    /// </summary>
    [TestFixture]
    public class PriceCalculationTests
    {
        /// <summary>
        /// Pomocná metoda pro výpočet výsledné ceny.
        /// Pokud máš ve své aplikaci jiný vzorec,
        /// stačí upravit tuto metodu.
        /// </summary>
        private double CalculateFinalPrice(PriceItems item)
        {
            return item.BasePrice * item.MaterialCoef * item.PositionCoef;
        }

        // ============================================================
        // TEST 1 – ZÁKLADNÍ VÝPOČET
        // ============================================================

        /// <summary>
        /// Ověří, že základní výpočet funguje správně.
        /// </summary>
        [Test]
        public void FinalPrice_Should_Calculate_Correctly()
        {
            var item = new PriceItems
            {
                BasePrice = 100,
                MaterialCoef = 1.2,
                PositionCoef = 1.5
            };

            var result = CalculateFinalPrice(item);

            Assert.That(result, Is.EqualTo(180)); // 100 × 1.2 × 1.5
        }

        // ============================================================
        // TEST 2 – KOEFICIENTY = 1
        // ============================================================

        /// <summary>
        /// Ověří, že koeficienty 1 nemění cenu.
        /// </summary>
        [Test]
        public void FinalPrice_Should_Not_Change_When_Coefs_Are_One()
        {
            var item = new PriceItems
            {
                BasePrice = 250,
                MaterialCoef = 1,
                PositionCoef = 1
            };

            var result = CalculateFinalPrice(item);

            Assert.That(result, Is.EqualTo(250));
        }

        // ============================================================
        // TEST 3 – KOEFICIENTY < 1 (SLEVY)
        // ============================================================

        /// <summary>
        /// Ověří, že koeficienty menší než 1 snižují cenu.
        /// </summary>
        [Test]
        public void FinalPrice_Should_Decrease_With_Lower_Coefs()
        {
            var item = new PriceItems
            {
                BasePrice = 200,
                MaterialCoef = 0.8,
                PositionCoef = 0.9
            };

            var result = CalculateFinalPrice(item);

            Assert.That(result, Is.EqualTo(144)); // 200 × 0.8 × 0.9
        }

        // ============================================================
        // TEST 4 – KOEFICIENTY > 1 (ZVÝŠENÍ CENY)
        // ============================================================

        /// <summary>
        /// Ověří, že koeficienty větší než 1 zvyšují cenu.
        /// </summary>
        [Test]
        public void FinalPrice_Should_Increase_With_Higher_Coefs()
        {
            var item = new PriceItems
            {
                BasePrice = 80,
                MaterialCoef = 1.5,
                PositionCoef = 2.0
            };

            var result = CalculateFinalPrice(item);

            Assert.That(result, Is.EqualTo(240)); // 80 × 1.5 × 2
        }

        // ============================================================
        // TEST 5 – ZAOKROUHLENÍ
        // ============================================================

        /// <summary>
        /// Ověří, že výsledek lze správně zaokrouhlit na 2 desetinná místa.
        /// </summary>
        [Test]
        public void FinalPrice_Should_Round_To_Two_Decimals()
        {
            var item = new PriceItems
            {
                BasePrice = 123.456,
                MaterialCoef = 1.234,
                PositionCoef = 0.987
            };

            var result = CalculateFinalPrice(item);

            var rounded = Math.Round(result, 2);

            Assert.That(rounded, Is.EqualTo(150.42));
        }

        // ============================================================
        // TEST 6 – NULOVÉ HODNOTY
        // ============================================================

        /// <summary>
        /// Ověří, že nulová základní cena dává nulový výsledek.
        /// </summary>
        [Test]
        public void FinalPrice_Should_Be_Zero_When_BasePrice_Is_Zero()
        {
            var item = new PriceItems
            {
                BasePrice = 0,
                MaterialCoef = 1.5,
                PositionCoef = 2.0
            };

            var result = CalculateFinalPrice(item);

            Assert.That(result, Is.EqualTo(0));
        }

        // ============================================================
        // TEST 7 – NEGATIVNÍ HODNOTY
        // ============================================================

        /// <summary>
        /// Ověří, že negativní hodnoty se správně násobí.
        /// (Pokud to aplikace nemá povolit, lze přidat validaci.)
        /// </summary>
        [Test]
        public void FinalPrice_Should_Handle_Negative_Values()
        {
            var item = new PriceItems
            {
                BasePrice = -100,
                MaterialCoef = 1.2,
                PositionCoef = 1.5
            };

            var result = CalculateFinalPrice(item);

            Assert.That(result, Is.EqualTo(-180));
        }
    }
}
