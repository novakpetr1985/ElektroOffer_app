﻿using ElektroOffer_app.Models;
using ElektroOffer_app.Services.Abstractions;   // 🧩 Rozhraní pro DI (mockovatelné v testech)
using System.IO;                                // 📄 File.ReadAllText / WriteAllText
using System.Text.Json;                         // 🔧 JSON serializace
using System.Windows;                           // MessageBox typy používané přes IMessageBoxService

namespace ElektroOffer_app.Services
{
    // ============================================================================
    // 🏗️ ProjectService – centrální služba pro práci s projektovými soubory (.eof)
    // ============================================================================
    //
    // Tato služba je jediným místem, které pracuje se soubory projektu:
    //
    //   ✔ Ukládání projektu (Save / SaveAs)
    //   ✔ Načítání projektu (Load)
    //   ✔ Kontrola neuložených změn (ConfirmNewProject)
    //
    // Architektura:
    //   • UI pracuje s MainViewModel → ten sestaví ProjectData
    //   • ProjectService pracuje výhradně s ProjectData (datový model)
    //   • ProjectData je čistý JSON-friendly model bez logiky
    //
    // ============================================================================

    /// <summary>
    /// Serializuje celý projekt do .eof a koordinuje dialogy, souborový systém a hlášení chyb.
    /// </summary>
    public class ProjectService
    {
        // ------------------------------------------------------------------------
        // 🔌 DI závislosti – nullable, protože existuje defaultní konstruktor
        // ------------------------------------------------------------------------
        private readonly IFileDialogService? _dialogs;   // 🧩 Abstrakce pro dialogy
        private readonly IFileSystemService? _fs;        // 🧩 Abstrakce pro File.Read/Write
        private readonly IMessageBoxService? _msg;       // 🧩 Abstrakce pro MessageBox

        // ------------------------------------------------------------------------
        // 🧪 DI konstruktor – používají ho testy (Mock<>)
        // ------------------------------------------------------------------------
        public ProjectService(
            IFileDialogService dialogs,
            IFileSystemService fs,
            IMessageBoxService msg)
        {
            _dialogs = dialogs;
            _fs = fs;
            _msg = msg;
        }

        // ------------------------------------------------------------------------
        // 🖥️ Defaultní konstruktor – používá ho aplikace (UI)
        // ------------------------------------------------------------------------
        public ProjectService() { }

        // ------------------------------------------------------------------------
        // ⚙️ JSON nastavení – formátovaný výstup pro čitelnost
        // ------------------------------------------------------------------------
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        // ============================================================================
        // 💾 SAVE (Ctrl+S)
        // ============================================================================
        //
        // Ukládá projekt na existující cestu.
        // Pokud projekt ještě nemá cestu → volá SaveAs().
        //
        // ============================================================================
        public string? Save(ProjectData data, string? currentPath)
        {
            if (string.IsNullOrEmpty(currentPath))
                return SaveAs(data);

            return SaveToPath(data, currentPath);
        }

        // ============================================================================
        // 💾 SAVE AS (Ctrl+Shift+S)
        // ============================================================================
        //
        // Otevře dialog pro výběr cesty a uloží projekt na novou cestu.
        //
        // ============================================================================
        public string? SaveAs(ProjectData data, string? currentPath = null)
        {
            EnsureDialogService();

            var path = _dialogs!.ShowSaveFileDialog(
                "Projekt ElektroOffer (*.eof)|*.eof",
                "Uložit projekt",
                ".eof",
                data.ProjectName);

            if (path == null)
                return null;

            if (!string.IsNullOrWhiteSpace(currentPath)
                && !string.Equals(Path.GetFullPath(currentPath), Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    CopyProjectAssets(currentPath, path);
                }
                catch (Exception ex)
                {
                    _msg!.Show($"Přílohy projektu nelze zkopírovat:\n{ex.Message}", "Chyba",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }

            return SaveToPath(data, path);
        }

        // ============================================================================
        // 📂 LOAD (Ctrl+O)
        // ============================================================================
        //
        // Načte projekt ze souboru .eof.
        // Vrací dvojici (ProjectData, path).
        //
        // ============================================================================
        public (ProjectData? data, string? path) Load()
        {
            EnsureDialogService();
            EnsureFileSystemService();
            EnsureMessageBoxService();

            var path = _dialogs!.ShowOpenFileDialog(
                "Projekt ElektroOffer (*.eof)|*.eof",
                "Načíst projekt");

            if (path == null)
                return (null, null);

            try
            {
                var json = _fs!.ReadAllText(path);
                var data = JsonSerializer.Deserialize<ProjectData>(json, _jsonOptions);

                if (data == null)
                {
                    _msg!.Show("Soubor nelze načíst – poškozený formát.", "Chyba",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return (null, null);
                }

                return (data, path);
            }
            catch (Exception ex)
            {
                _msg!.Show($"Chyba při načítání:\n{ex.Message}", "Chyba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return (null, null);
            }
        }

        // ============================================================================
        // ⚠️ Kontrola neuložených změn
        // ============================================================================
        //
        // Používá se při:
        //   • vytvoření nového projektu
        //   • otevření jiného projektu
        //
        // Vrací:
        //   ✔ true  → pokračovat
        //   ✔ false → zrušit akci
        //
        // ============================================================================
        public bool ConfirmNewProject(ProjectData data, string? currentPath, bool hasUnsavedChanges)
        {
            EnsureMessageBoxService();

            if (!hasUnsavedChanges)
                return true;

            var result = _msg!.Show(
                "Projekt obsahuje neuložené změny.\nUložit před vytvořením nového projektu?",
                "Neuložené změny",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            return result switch
            {
                MessageBoxResult.Yes => Save(data, currentPath) != null,
                MessageBoxResult.No => true,
                _ => false
            };
        }

        // ============================================================================
        // 💾 Interní metoda – ukládání JSON na cestu
        // ============================================================================
        //
        // Používá se v Save() a SaveAs().
        //
        // ============================================================================
        private string? SaveToPath(ProjectData data, string path)
        {
            EnsureFileSystemService();
            EnsureMessageBoxService();

            try
            {
                data.SavedAt = DateTime.Now;

                var json = JsonSerializer.Serialize(data, _jsonOptions);
                _fs!.WriteAllText(path, json);

                return path;
            }
            catch (Exception ex)
            {
                _msg!.Show($"Chyba ukládání:\n{ex.Message}", "Chyba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private static void CopyProjectAssets(string sourceProjectPath, string destinationProjectPath)
        {
            var source = MeasurementImportService.GetProjectAssetsDirectory(sourceProjectPath);
            if (!Directory.Exists(source)) return;
            var destination = MeasurementImportService.GetProjectAssetsDirectory(destinationProjectPath);
            Directory.CreateDirectory(destination);
            foreach (var directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, directory)));
            foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                var target = Path.Combine(destination, Path.GetRelativePath(source, file));
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target, overwrite: true);
            }
        }

        // ============================================================================
        // 🔒 Ochranné metody – zajišťují, že DI služby nejsou null
        // ============================================================================
        private void EnsureDialogService()
        {
            if (_dialogs == null)
                throw new InvalidOperationException("IFileDialogService is not configured.");
        }

        private void EnsureFileSystemService()
        {
            if (_fs == null)
                throw new InvalidOperationException("IFileSystemService is not configured.");
        }

        private void EnsureMessageBoxService()
        {
            if (_msg == null)
                throw new InvalidOperationException("IMessageBoxService is not configured.");
        }
    }
}
