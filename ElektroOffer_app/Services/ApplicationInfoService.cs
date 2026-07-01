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

                // 1) Preferujeme InformationalVersion – obsahuje i suffixy (-dev, -beta, ...)
                var info = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                ?.InformationalVersion;

                if (!string.IsNullOrWhiteSpace(info))
                {
                    // Odstraníme pouze automatický +commit_hash
                    var clean = info.Split('+')[0];
                    return clean;
                }

                // 2) Fallback – klasická AssemblyName.Version (bez suffixů)
                var v = assembly.GetName().Version;

                if (v != null)
                {
                    // Pokud máš patch (Build) i revision (Revision), zobrazíme oboje
                    if (v.Revision > 0)
                        return $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";

                    return $"{v.Major}.{v.Minor}.{v.Build}";
                }

                return "neznámá";
            }
        }
    }
}