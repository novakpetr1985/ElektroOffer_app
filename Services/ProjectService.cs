using ElektroOffer_app.Models;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace ElektroOffer_app.Services
{
    /// <summary>
    /// Servisní třída zodpovědná za:
    /// - Uložení projektu do JSON souboru (Save / Save As)
    /// - Načtení projektu z JSON souboru (Load)
    /// - Reset projektu (Nový projekt)
    ///
    /// Tato třída NEVÍ nic o UI — pouze pracuje s daty (ProjectData).
    /// Veškeré propojení s UI (ObservableCollection atd.) se děje v MainWindow.
    /// </summary>
    public class ProjectService
    {
        // =========================================================
        // ⚙️ NASTAVENÍ SERIALIZACE
        // =========================================================

        /// <summary>
        /// Možnosti pro System.Text.Json serializátor.
        /// WriteIndented = true → JSON je čitelný pro lidi (pěkně odsazený).
        /// </summary>
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        // =========================================================
        // 💾 SAVE — uložení na aktuální cestu (Ctrl+S)
        // =========================================================

        /// <summary>
        /// Uloží projekt na aktuální cestu.
        /// Pokud cesta ještě neexistuje (první uložení), zavolá SaveAs.
        /// </summary>
        /// <param name="data">Data projektu k uložení</param>
        /// <param name="currentPath">Aktuální cesta souboru (null = ještě neuloženo)</param>
        /// <returns>Cesta, kam se uložilo (nebo null při zrušení dialogu)</returns>
        public string? Save(ProjectData data, string? currentPath)
        {
            // Pokud ještě nemáme cestu → chová se jako Save As
            if (string.IsNullOrEmpty(currentPath))
                return SaveAs(data);

            return SaveToPath(data, currentPath);
        }

        // =========================================================
        // 💾 SAVE AS — uložení s výběrem cesty (Ctrl+Shift+S)
        // =========================================================

        /// <summary>
        /// Otevře dialog pro výběr cesty a uloží projekt do nového souboru.
        /// </summary>
        /// <param name="data">Data projektu k uložení</param>
        /// <returns>Cesta, kam se uložilo (nebo null při zrušení dialogu)</returns>
        public string? SaveAs(ProjectData data)
        {
            // Otevření dialogu pro uložení souboru
            var dialog = new SaveFileDialog
            {
                Title = "Uložit projekt",
                Filter = "Projekt ElektroOffer (*.eof)|*.eof|Všechny soubory (*.*)|*.*",
                DefaultExt = ".eof",
                FileName = data.ProjectName
            };

            // Pokud uživatel zruší dialog → vrátíme null
            if (dialog.ShowDialog() != true)
                return null;

            return SaveToPath(data, dialog.FileName);
        }

        // =========================================================
        // 📂 LOAD — načtení projektu ze souboru
        // =========================================================

        /// <summary>
        /// Otevře dialog pro výběr souboru a načte projekt z JSON.
        /// </summary>
        /// <returns>
        /// Tuple: (načtená data, cesta k souboru).
        /// Vrátí (null, null) pokud uživatel zruší dialog nebo nastane chyba.
        /// </returns>
        public (ProjectData? data, string? path) Load()
        {
            // Otevření dialogu pro výběr souboru
            var dialog = new OpenFileDialog
            {
                Title = "Načíst projekt",
                Filter = "Projekt ElektroOffer (*.eof)|*.eof|Všechny soubory (*.*)|*.*"
            };

            // Pokud uživatel zruší dialog → vrátíme null
            if (dialog.ShowDialog() != true)
                return (null, null);

            try
            {
                // Přečtení JSON souboru a deserializace
                var json = File.ReadAllText(dialog.FileName);
                var data = JsonSerializer.Deserialize<ProjectData>(json, _jsonOptions);

                if (data == null)
                {
                    MessageBox.Show("Soubor nelze načíst — pravděpodobně poškozený formát.",
                        "Chyba načítání", MessageBoxButton.OK, MessageBoxImage.Error);
                    return (null, null);
                }

                return (data, dialog.FileName);
            }
            catch (Exception ex)
            {
                // Chyba při čtení nebo parsování JSON
                MessageBox.Show($"Chyba při načítání souboru:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                return (null, null);
            }
        }

        // =========================================================
        // 🆕 NEW PROJECT — kontrola neuložených změn
        // =========================================================

        /// <summary>
        /// Zeptá se uživatele, zda chce uložit změny před resetem.
        /// </summary>
        /// <returns>
        /// True  = pokračovat (uloženo nebo uživatel zvolil "Ne")
        /// False = uživatel zrušil akci (kliknul "Storno")
        /// </returns>
        public bool ConfirmNewProject(ProjectData data, string? currentPath, bool hasUnsavedChanges)
        {
            // Pokud nejsou neuložené změny → rovnou pokračujeme
            if (!hasUnsavedChanges)
                return true;

            var result = MessageBox.Show(
                "Projekt obsahuje neuložené změny.\nChcete je uložit před vytvořením nového projektu?",
                "Neuložené změny",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            return result switch
            {
                // Uložit a pokračovat
                MessageBoxResult.Yes => Save(data, currentPath) != null,

                // Bez uložení pokračovat
                MessageBoxResult.No => true,

                // Zrušit akci
                _ => false
            };
        }

        // =========================================================
        // 📤 EXPORT CENÍKU — PriceItems + Materials do JSON
        // =========================================================

        /// <summary>
        /// Exportuje ceník práce (PriceItems) a materiálu (Materials) do JSON souboru.
        /// Otevře dialog pro výběr cesty a uloží data ve formátu .eofcat.
        /// </summary>
        /// <param name="data">Připravená exportní data (sestavená v MainWindow)</param>
        /// <returns>True = export proběhl úspěšně, False = zrušeno nebo chyba</returns>
        public bool ExportCatalog(CatalogExportData data)
        {
            // Otevření dialogu pro uložení souboru
            var dialog = new SaveFileDialog
            {
                Title = "Exportovat ceník",
                Filter = "Ceník ElektroOffer (*.eofcat)|*.eofcat|Všechny soubory (*.*)|*.*",
                DefaultExt = ".eofcat",
                FileName = $"cenik_export_{DateTime.Now:yyyy-MM-dd}"
            };

            // Pokud uživatel zruší dialog → vrátíme false
            if (dialog.ShowDialog() != true)
                return false;

            try
            {
                // Aktualizace času exportu
                data.ExportedAt = DateTime.Now;

                // Serializace do čitelného JSON
                var json = JsonSerializer.Serialize(data, _jsonOptions);

                // Zápis na disk
                File.WriteAllText(dialog.FileName, json);

                MessageBox.Show(
                    $"Ceník byl úspěšně exportován.\n\n" +
                    $"Položky práce: {data.PriceItems.Count}\n" +
                    $"Položky materiálu: {data.Materials.Count}\n\n" +
                    $"Soubor: {dialog.FileName}",
                    "Export dokončen",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při exportu ceníku:\n{ex.Message}",
                    "Chyba exportu", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // =========================================================
        // 📥 IMPORT CENÍKU — PriceItems + Materials z JSON
        // =========================================================

        /// <summary>
        /// Importuje ceník ze souboru .eofcat (JSON).
        /// Otevře dialog pro výběr souboru a vrátí načtená data.
        /// Samotný zápis do DB provádí MainWindow (má přístup k AppDbContext).
        /// </summary>
        /// <returns>Načtená data ceníku, nebo null při zrušení/chybě</returns>
        public CatalogExportData? ImportCatalog()
        {
            // Otevření dialogu pro výběr souboru
            var dialog = new OpenFileDialog
            {
                Title = "Importovat ceník",
                Filter = "Ceník ElektroOffer (*.eofcat)|*.eofcat|Všechny soubory (*.*)|*.*"
            };

            // Pokud uživatel zruší dialog → vrátíme null
            if (dialog.ShowDialog() != true)
                return null;

            try
            {
                // Přečtení a deserializace JSON souboru
                var json = File.ReadAllText(dialog.FileName);
                var data = JsonSerializer.Deserialize<CatalogExportData>(json, _jsonOptions);

                if (data == null)
                {
                    MessageBox.Show(
                        "Soubor nelze načíst — pravděpodobně poškozený nebo neplatný formát.",
                        "Chyba importu", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                return data;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při importu ceníku:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        // =========================================================
        // 🔒 PRIVÁTNÍ: Zápis JSON na disk
        // =========================================================

        /// <summary>
        /// Interní metoda — serializuje data do JSON a zapíše na disk.
        /// </summary>
        private string? SaveToPath(ProjectData data, string path)
        {
            try
            {
                // Aktualizace data uložení
                data.SavedAt = DateTime.Now;

                // Serializace do JSON stringu
                var json = JsonSerializer.Serialize(data, _jsonOptions);

                // Zápis na disk
                File.WriteAllText(path, json);

                return path;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při ukládání souboru:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
