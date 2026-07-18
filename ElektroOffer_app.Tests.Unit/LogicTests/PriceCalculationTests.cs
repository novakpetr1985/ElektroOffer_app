using ElektroOffer_app.Models;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.LogicTests
{
    [TestFixture]
    /// <summary>Ověřuje základní skládání cen práce a materiálu.</summary>
    public class PriceCalculationTests
    {
        private static double CalculateWorkTotal(
            WorkTask task,
            BaseMaterial baseMaterial,
            WorkPosition position,
            double quantity)
        {
            return (double)task.BasePrice
                 * (double)baseMaterial.BaseMaterialCoef
                 * (double)position.PositionCoef
                 * quantity;
        }

        [Test]
        public void WorkTotal_Should_Calculate_From_New_Cascade_Entities()
        {
            var result = CalculateWorkTotal(
                new WorkTask { BasePrice = 100m },
                new BaseMaterial { BaseMaterialCoef = 1.2m },
                new WorkPosition { PositionCoef = 1.5m },
                2);

            Assert.That(result, Is.EqualTo(360d));
        }

        [Test]
        public void WorkTotal_Should_Be_Zero_When_Quantity_Is_Zero()
        {
            var result = CalculateWorkTotal(
                new WorkTask { BasePrice = 100m },
                new BaseMaterial { BaseMaterialCoef = 1.2m },
                new WorkPosition { PositionCoef = 1.5m },
                0);

            Assert.That(result, Is.EqualTo(0d));
        }
    }
}
