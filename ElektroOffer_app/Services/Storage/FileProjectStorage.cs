using ElektroOffer_app.Models;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace ElektroOffer_app.Services.Storage
{
    /// <summary>
    /// KONKRÉTNÍ implementace ukládání na disk (soubor .eof)
    /// </summary>
    /// 
    /// 👉 PROČ EXISTUJE:
    /// - oddělení File I/O z ProjectService
    /// - izolace JSON serializace
    /// - snadná výměna za DB / API v budoucnu
    public class FileProjectStorage : IProjectStorage
    {
        // ------------------------------------------------------------
        // JSON nastavení (sdílené pro Save i Load)
        // ------------------------------------------------------------
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        // ------------------------------------------------------------
        // SAVE
        // ------------------------------------------------------------
        public string? Save(ProjectData data, string path)
        {
            try
            {
                // 🟡 aktualizace metadat projektu
                data.SavedAt = DateTime.Now;

                // 🟢 převod objektu na JSON text
                var json = JsonSerializer.Serialize(data, _jsonOptions);

                // 🟢 zápis do souboru
                File.WriteAllText(path, json);

                // ✔ úspěch → vracíme cestu
                return path;
            }
            catch (Exception ex)
            {
                // 🔴 UI ALERT (dočasně zde, později DialogService)
                MessageBox.Show(
                    $"Chyba ukládání:\n{ex.Message}",
                    "Chyba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                return null;
            }
        }

        // ------------------------------------------------------------
        // LOAD
        // ------------------------------------------------------------
        public (ProjectData? data, string? path) Load(string path)
        {
            try
            {
                // 🟢 načtení textu ze souboru
                var json = File.ReadAllText(path);

                // 🟢 deserializace JSON → objekt
                var data = JsonSerializer.Deserialize<ProjectData>(json, _jsonOptions);

                // 🔴 ochrana proti poškozenému souboru
                if (data == null)
                {
                    MessageBox.Show(
                        "Soubor nelze načíst – poškozený formát.",
                        "Chyba",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );

                    return (null, null);
                }

                // ✔ úspěch
                return (data, path);
            }
            catch (Exception ex)
            {
                // 🔴 error při čtení souboru
                MessageBox.Show(
                    $"Chyba při načítání:\n{ex.Message}",
                    "Chyba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                return (null, null);
            }
        }
    }
}