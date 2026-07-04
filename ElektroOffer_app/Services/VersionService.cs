using System.Reflection;

namespace ElektroOffer_app.Services
{
    public static class VersionService
    {
        /// <summary>
        /// Vrací verzi hlavní aplikace z assembly metadata.
        /// Používá Assembly.GetEntryAssembly(), aby nevracel verzi testovací assembly.
        /// </summary>
        public static string GetAppVersion()
        {
            return Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown";
        }
    }
}
