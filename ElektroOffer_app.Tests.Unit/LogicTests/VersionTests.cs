using NUnit.Framework;
using ElektroOffer_app.Services;

namespace ElektroOffer_app.Tests.Unit.LogicTests
{
    /// <summary>
    /// Unit testy ověřující, že aplikace má správně nastavenou verzi
    /// v assembly metadata. Testy slouží jako sanity-check build pipeline
    /// a zajišťují, že verze není prázdná, má validní formát a není defaultní.
    /// </summary>
    public class VersionTests
    {
        /// <summary>
        /// Ověřuje, že verze aplikace není null, prázdná nebo whitespace.
        /// Pokud by build pipeline nezapsala verzi do csproj, test selže.
        /// </summary>
        [Test]
        public void AppVersion_IsNotNullOrEmpty()
        {
            var version = VersionService.GetAppVersion();

            Assert.IsFalse(string.IsNullOrWhiteSpace(version),
                "Assembly version is null or empty. Build pipeline may have failed to set <Version> in csproj.");
        }

        /// <summary>
        /// Ověřuje, že verze má validní formát (např. 1.7.5).
        /// Používá System.Version.TryParse, který kontroluje major/minor/build/revision.
        /// </summary>
        [Test]
        public void AppVersion_HasValidFormat()
        {
            var version = VersionService.GetAppVersion();

            Assert.That(Version.TryParse(version, out _), Is.True,
                $"Assembly version '{version}' is not in a valid format.");
        }

        /// <summary>
        /// Ověřuje, že verze není defaultní 1.0.0.0.
        /// Defaultní verze znamená, že csproj neobsahuje <Version> nebo build pipeline ji nepřepsala.
        /// </summary>
        [Test]
        public void AppVersion_IsNotDefault()
        {
            var version = VersionService.GetAppVersion();

            Assert.AreNotEqual("1.0.0.0", version,
                "Assembly version is default (1.0.0.0). <Version> may be missing in csproj.");
        }
    }
}
