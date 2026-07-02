using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.LogicTests
{
    /// <summary>
    /// UNIT TESTY:
    /// Testují logiku slev (discount), jak se typicky používá ve ViewModelu.
    /// 
    /// Vzorec:
    /// basePrice × (1 - percent/100)
    /// </summary>
    [TestFixture]
    public class DiscountCalculationTests
    {
        /// <summary>
        /// Simulace slevové logiky (ViewModel / Service).
        /// </summary>
        private double ApplyDiscount(double basePrice, double percent)
        {
            return basePrice * (1 - percent / 100.0);
        }

        // ============================================================
        // TEST 1 – SLEVA 10 %
        // ============================================================

        [Test]
        public void Discount_Should_Apply_10Percent_Correctly()
        {
            var result = ApplyDiscount(100, 10);

            Assert.That(result, Is.EqualTo(90));
        }

        // ============================================================
        // TEST 2 – 0 % SLEVA
        // ============================================================

        [Test]
        public void Discount_Should_Be_Ignored_When_Zero()
        {
            var result = ApplyDiscount(100, 0);

            Assert.That(result, Is.EqualTo(100));
        }

        // ============================================================
        // TEST 3 – 100 % SLEVA
        // ============================================================

        [Test]
        public void Discount_Should_Zero_Out_Price()
        {
            var result = ApplyDiscount(100, 100);

            Assert.That(result, Is.EqualTo(0));
        }

        // ============================================================
        // TEST 4 – EDGE CASE > 100 %
        // ============================================================

        /// <summary>
        /// Ověří chování při slevě větší než 100 %.
        /// (aplikace může později omezit validací 0–100 %)
        /// </summary>
        [Test]
        public void Discount_Should_Handle_Over_100Percent()
        {
            var result = ApplyDiscount(100, 150);

            Assert.That(result, Is.EqualTo(-50));
        }
    }
}
