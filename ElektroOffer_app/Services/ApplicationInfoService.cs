using System.Reflection;

namespace ElektroOffer_app.Services
{
    // =========================================================================
    // ℹ ApplicationInfoService – informace o aplikaci a verzi
    // =========================================================================
    //
    // ÚČEL:
    // - Poskytuje jednotné místo pro čtení metadat aplikace.
    // - Verze se bere z assembly atributů generovaných z hlavního .csproj.
    // - Díky tomu není potřeba držet verzi ručně v About okně, ViewModelech
    //   ani dalších částech aplikace.
    //
    // JAK VERZOVAT:
    // - Při nové verzi změň hodnotu <Version> v ElektroOffer_app.csproj.
    // - Stejnou verzi doplň do docs/README.md a docs/CHANGELOG.md.
    // - Okno "O aplikaci" i ViewModely si verzi načtou automaticky z buildu.
    //
    // =========================================================================
    public static class ApplicationInfoService
    {
        // =====================================================================
        // HLAVNÍ: Aktuální verze aplikace
        // =====================================================================

        /// <summary>
        /// Vrátí uživatelskou verzi aplikace.
        /// Primárně čte AssemblyInformationalVersion, která odpovídá hodnotě
        /// InformationalVersion / Version z hlavního .csproj.
        /// </summary>
        public static string Version
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();

                var informationalVersion = assembly
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion;

                // .NET může za verzi přidat "+commit_hash" suffix (např. "1.7.0+a3f2c8b")
                // Split('+')[0] vezme pouze část před '+', tedy čisté "1.7.0"
                if (!string.IsNullOrWhiteSpace(informationalVersion))
                    return informationalVersion.Split('+')[0];

                // Záložní čtení přes AssemblyName – vrátí např. "1.7.0"
                var version = assembly.GetName().Version;

                return version != null
                    ? $"{version.Major}.{version.Minor}.{version.Build}"
                    : "neznámá";
            }
        }
    }
}