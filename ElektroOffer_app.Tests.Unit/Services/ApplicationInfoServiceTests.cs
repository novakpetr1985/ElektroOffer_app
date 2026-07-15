using ElektroOffer_app.Services;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.Services
{
    [TestFixture]
    public class ApplicationInfoServiceTests
    {
        [Test]
        public void Version_Should_Not_Be_Empty()
        {
            Assert.That(ApplicationInfoService.Version, Is.Not.Empty);
        }

        [Test]
        public void Version_Should_Have_Valid_Numeric_Format()
        {
            var numericVersion = ApplicationInfoService.Version.Split('-')[0];

            Assert.That(Version.TryParse(numericVersion, out _), Is.True);
        }

        [Test]
        public void Version_Should_Not_Use_Default_Assembly_Value()
        {
            Assert.That(ApplicationInfoService.Version, Is.Not.EqualTo("1.0.0.0"));
        }
    }
}
