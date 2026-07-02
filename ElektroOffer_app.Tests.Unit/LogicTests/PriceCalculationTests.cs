using ElektroOffer_app.Models;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.LogicTests
{
    /// <summary>
    /// UNIT TESTY:
    /// Testují čistý matematický výpočet ceny z modelu PriceItems.
    /// 
    /// Vzorec:
    /// BasePrice × MaterialCoef × PositionCoef
    /// </summary>
    [TestFixture]
    public class PriceCalculationTests
    {
        /// <summary>
        /// Pomocná metoda pro výpočet ceny.
        /// (Zrcadlí logiku aplikace – pokud se změní, upravit i zde.)
        /// </summary>
        private double CalculateFinalPrice(PriceItems item)
        {
            return item.BasePrice * item.MaterialCoef * item.PositionCoef;
        }

        // ============================================================
        // TEST 1 – ZÁKLADNÍ VÝPOČET
        // ============================================================

        /// <summary>
        /// Ověří správný základní výpočet ceny.
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
        /// Ověří, že koeficienty = 1 nemění cenu.
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
        // TEST 3 – SLEVY (KOEF < 1)
        // ============================================================

        /// <summary>
        /// Ověří snížení ceny při koeficientech < 1.
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

            Assert.That(result, Is.EqualTo(144));
        }

        // ============================================================
        // TEST 4 – NAVÝŠENÍ (KOEF > 1)
        // ============================================================

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

            Assert.That(result, Is.EqualTo(240));
        }
/*
        // ============================================================
        // TEST 5 – ZAOKROUHLENÍ
        // ============================================================

        /// <summary>
        /// Ověří zaokrouhlení výsledku (pokud UI pracuje s decimal/double).
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
*/
        // ============================================================
        // TEST 6 – NULOVÁ CENA
        // ============================================================

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
        // TEST 7 – NEGATIVNÍ HODNOTY (EDGE CASE)
        // ============================================================

        /// <summary>
        /// Ověří chování při záporné ceně.
        /// (Buď povoleno, nebo později nahradit validací.)
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