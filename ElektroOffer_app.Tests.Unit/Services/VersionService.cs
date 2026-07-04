using NUnit.Framework;
using ElektroOffer_app.Services;

namespace ElektroOffer_app.Tests.Unit.LogicTests
{
    /// =====================================================================
    /// 🧪 UNIT TESTS — VersionService
    /// =====================================================================
    /// Tyto testy ověřují, že aplikace má správně nastavenou verzi v assembly
    /// metadata. Jde o tzv. "sanity-check" testy build pipeline:
    ///
    ///   • ověřují, že build skutečně zapsal verzi do assembly
    ///   • chrání proti chybě, kdy <Version> v csproj chybí
    ///   • chrání proti chybě, kdy CI pipeline nepřepsala verzi
    ///   • ověřují validní formát verze (major.minor.patch)
    ///   • ověřují, že verze není defaultní 1.0.0.0
    ///
    /// Tyto testy jsou extrémně užitečné při CI/CD:
    ///   • pokud pipeline zapomene přepsat verzi → test okamžitě selže
    ///   • pokud je csproj špatně nastaven → test okamžitě selže
    ///   • pokud je verze prázdná → test okamžitě selže
    ///
    /// =====================================================================
    public class VersionTests
    {
        /// -----------------------------------------------------------------
        /// 🧪 TEST 1: Verze není null, prázdná ani whitespace
        /// -----------------------------------------------------------------
        /// Ověřuje:
        ///   • že VersionService skutečně načetl verzi z assembly metadata
        ///   • že build pipeline zapsala verzi do csproj (<Version>)
        ///   • že verze není prázdná (což by znamenalo chybu v build procesu)
        ///
        /// Tento test odhalí chyby typu:
        ///   • chybějící <Version> v csproj
        ///   • chybné nastavení CI pipeline (např. zapomenutý step)
        ///   • chybné načítání verze v samotném VersionService
        [Test]
        public void AppVersion_IsNotNullOrEmpty()
        {
            var version = VersionService.GetAppVersion();

            Assert.IsFalse(string.IsNullOrWhiteSpace(version),
                "Assembly version is null or empty. Build pipeline may have failed to set <Version> in csproj.");
        }

        /// -----------------------------------------------------------------
        /// 🧪 TEST 2: Verze má validní formát
        /// -----------------------------------------------------------------
        /// Ověřuje:
        ///   • že verze odpovídá formátu major.minor.build.revision
        ///   • že System.Version.TryParse verzi úspěšně zpracuje
        ///
        /// Tento test odhalí chyby typu:
        ///   • verze obsahuje nevalidní znaky (např. '1.7.5-feature')
        ///   • verze má špatný počet segmentů
        ///   • verze není číslo
        ///
        /// Poznámka:
        ///   Pokud používáš <AssemblyInformationalVersion> s suffixem (např. 1.7.5-feature),
        ///   tento test bude selhávat — a je to správně, protože VersionService
        ///   vrací AssemblyVersion, ne InformationalVersion.
        [Test]
        public void AppVersion_HasValidFormat()
        {
            var version = VersionService.GetAppVersion();

            Assert.That(Version.TryParse(version, out _), Is.True,
                $"Assembly version '{version}' is not in a valid format.");
        }

        /// -----------------------------------------------------------------
        /// 🧪 TEST 3: Verze není defaultní 1.0.0.0
        /// -----------------------------------------------------------------
        /// Ověřuje:
        ///   • že verze není defaultní hodnota assembly
        ///   • že csproj obsahuje <Version>
        ///   • že build pipeline správně přepisuje verzi
        ///
        /// Tento test odhalí chyby typu:
        ///   • chybějící <Version> v csproj
        ///   • chybné nastavení CI pipeline (např. chybějící step)
        ///   • build běží mimo pipeline a nepřepisuje verzi
        [Test]
        public void AppVersion_IsNotDefault()
        {
            var version = VersionService.GetAppVersion();

            Assert.AreNotEqual("1.0.0.0", version,
                "Assembly version is default (1.0.0.0). <Version> may be missing in csproj.");
        }
    }
}
