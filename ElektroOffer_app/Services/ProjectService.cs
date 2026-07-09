﻿using ElektroOffer_app.Models;
using ElektroOffer_app.Services.Abstractions;   // 🧩 Rozhraní pro DI (mockovatelné v testech)
using Microsoft.Win32;                          // 📁 OpenFileDialog / SaveFileDialog
using System.IO;                                // 📄 File.ReadAllText / WriteAllText
using System.Text.Json;                         // 🔧 JSON serializace
using System.Windows;                           // ⚠️ MessageBox (později nahradíme službou)

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
    //   ✔ Export / import ceníku (.eofcat)
    //
    // Architektura:
    //   • UI pracuje s MainViewModel → ten sestaví ProjectData
    //   • ProjectService pracuje výhradně s ProjectData (datový model)
    //   • ProjectData je čistý JSON-friendly model bez logiky
    //
    // Poznámka:
    //   • MessageBoxService je dočasné řešení – později bude nahrazeno UI službou
    //
    // ============================================================================

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
        public string? SaveAs(ProjectData data)
        {
            EnsureDialogService();

            var path = _dialogs!.ShowSaveFileDialog(
                "Projekt ElektroOffer (*.eof)|*.eof",
                "Uložit projekt",
                ".eof",
                data.ProjectName);

            if (path == null)
                return null;

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
        // 📤 Export ceníku (.eofcat)
        // ============================================================================
        //
        // Exportuje ceník do samostatného souboru.
        //
        // ============================================================================
        public bool ExportCatalog(CatalogExportData data)
        {
            EnsureDialogService();
            EnsureFileSystemService();
            EnsureMessageBoxService();

            var path = _dialogs!.ShowSaveFileDialog(
                "Ceník ElektroOffer (*.eofcat)|*.eofcat",
                "Exportovat ceník",
                ".eofcat",
                $"cenik_export_{DateTime.Now:yyyy-MM-dd}");

            if (path == null)
                return false;

            try
            {
                data.ExportedAt = DateTime.Now;

                var json = JsonSerializer.Serialize(data, _jsonOptions);
                _fs!.WriteAllText(path, json);

                _msg!.Show(
                    $"Export dokončen.\n\nPráce: {data.PriceItems.Count}\nMateriál: {data.Materials.Count}",
                    "Hotovo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return true;
            }
            catch (Exception ex)
            {
                _msg!.Show($"Chyba exportu:\n{ex.Message}", "Chyba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // ============================================================================
        // 📥 Import ceníku (.eofcat)
        // ============================================================================
        //
        // Načte ceník ze souboru .eofcat.
        //
        // ============================================================================
        public CatalogExportData? ImportCatalog()
        {
            EnsureDialogService();
            EnsureFileSystemService();
            EnsureMessageBoxService();

            var path = _dialogs!.ShowOpenFileDialog(
                "Ceník ElektroOffer (*.eofcat)|*.eofcat",
                "Importovat ceník");

            if (path == null)
                return null;

            try
            {
                var json = _fs!.ReadAllText(path);
                return JsonSerializer.Deserialize<CatalogExportData>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                _msg!.Show($"Chyba importu:\n{ex.Message}", "Chyba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
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
