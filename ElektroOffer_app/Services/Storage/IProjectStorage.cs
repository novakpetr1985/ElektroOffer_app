using ElektroOffer_app.Models;

namespace ElektroOffer_app.Services.Storage
{
    /// <summary>
    /// Abstrakce pro ukládání a načítání projektu.
    /// </summary>
    /// 
    /// 👉 PROČ EXISTUJE:
    /// - aby ProjectService NEZNAL file system
    /// - aby šel snadno vyměnit způsob ukládání (např. DB, cloud)
    /// - aby šly testy mockovat
    public interface IProjectStorage
    {
        /// <summary>
        /// Uloží projekt na zadanou cestu.
        /// </summary>
        /// <returns>
        /// Vrací cestu pokud úspěch, jinak null
        /// </returns>
        string? Save(ProjectData data, string path);

        /// <summary>
        /// Načte projekt ze souboru.
        /// </summary>
        /// <returns>
        /// tuple:
        /// - data = načtený projekt
        /// - path = potvrzená cesta
        /// </returns>
        (ProjectData? data, string? path) Load(string path);
    }
}