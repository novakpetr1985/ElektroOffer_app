﻿using ElektroOffer_app.Models;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace ElektroOffer_app.Services
{
    // ========================================================================
    // HLAVNÍ: ProjectService – správa projektových souborů (.eof)
    // ========================================================================
    //
    // ÚČEL:
    // - Ukládání projektu do JSON (Save / SaveAs)
    // - Načítání projektu z JSON (Load)
    // - Kontrola neuložených změn (ConfirmNewProject)
    // - Export a import ceníku (.eofcat)
    //
    // DŮLEŽITÉ:
    // - Třída NEPRACUJE s UI (kromě MessageBox – lze později nahradit DialogService)
    // - Třída NEZNÁ ViewModel ani UI logiku
    // - Používá pouze ProjectData a CatalogExportData
    //
    // ========================================================================
    public class ProjectService
    {
        // --------------------------------------------------------------------
        // DETAILNÍ: Nastavení JSON serializace
        // --------------------------------------------------------------------
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        // --------------------------------------------------------------------
        // HLAVNÍ: SAVE (Ctrl+S)
        // --------------------------------------------------------------------
        public string? Save(ProjectData data, string? currentPath)
        {
            if (string.IsNullOrEmpty(currentPath))
                return SaveAs(data);

            return SaveToPath(data, currentPath);
        }

        // --------------------------------------------------------------------
        // HLAVNÍ: SAVE AS (Ctrl+Shift+S)
        // --------------------------------------------------------------------
        public string? SaveAs(ProjectData data)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Uložit projekt",
                Filter = "Projekt ElektroOffer (*.eof)|*.eof",
                DefaultExt = ".eof",
                FileName = data.ProjectName
            };

            if (dialog.ShowDialog() != true)
                return null;

            return SaveToPath(data, dialog.FileName);
        }

        // --------------------------------------------------------------------
        // HLAVNÍ: LOAD (Ctrl+O)
        // --------------------------------------------------------------------
        public (ProjectData? data, string? path) Load()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Načíst projekt",
                Filter = "Projekt ElektroOffer (*.eof)|*.eof"
            };

            if (dialog.ShowDialog() != true)
                return (null, null);

            try
            {
                var json = File.ReadAllText(dialog.FileName);
                var data = JsonSerializer.Deserialize<ProjectData>(json, _jsonOptions);

                if (data == null)
                {
                    MessageBox.Show("Soubor nelze načíst – poškozený formát.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return (null, null);
                }

                return (data, dialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při načítání:\n{ex.Message}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                return (null, null);
            }
        }

        // --------------------------------------------------------------------
        // VEDLEJŠÍ: Kontrola neuložených změn
        // --------------------------------------------------------------------
        /// Předává se zvenčí, protože ProjectService nezná stav UI —
        /// o neuložených změnách ví jen MainWindow.
        public bool ConfirmNewProject(ProjectData data, string? currentPath, bool hasUnsavedChanges)
        {
            if (!hasUnsavedChanges)
                return true;

            var result = MessageBox.Show(
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

        // --------------------------------------------------------------------
        // HLAVNÍ: Export ceníku (.eofcat)
        // --------------------------------------------------------------------
        public bool ExportCatalog(CatalogExportData data)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Exportovat ceník",
                Filter = "Ceník ElektroOffer (*.eofcat)|*.eofcat",
                DefaultExt = ".eofcat",
                FileName = $"cenik_export_{DateTime.Now:yyyy-MM-dd}"
            };

            if (dialog.ShowDialog() != true)
                return false;

            try
            {
                data.ExportedAt = DateTime.Now;

                var json = JsonSerializer.Serialize(data, _jsonOptions);
                File.WriteAllText(dialog.FileName, json);

                MessageBox.Show(
                    $"Export dokončen.\n\nPráce: {data.PriceItems.Count}\nMateriál: {data.Materials.Count}",
                    "Hotovo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba exportu:\n{ex.Message}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // --------------------------------------------------------------------
        // HLAVNÍ: Import ceníku (.eofcat)
        // --------------------------------------------------------------------
        public CatalogExportData? ImportCatalog()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Importovat ceník",
                Filter = "Ceník ElektroOffer (*.eofcat)|*.eofcat"
            };

            if (dialog.ShowDialog() != true)
                return null;

            try
            {
                var json = File.ReadAllText(dialog.FileName);
                return JsonSerializer.Deserialize<CatalogExportData>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba importu:\n{ex.Message}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        // --------------------------------------------------------------------
        // DETAILNÍ: Interní metoda pro zápis JSON
        // --------------------------------------------------------------------
        private string? SaveToPath(ProjectData data, string path)
        {
            try
            {
                data.SavedAt = DateTime.Now;

                var json = JsonSerializer.Serialize(data, _jsonOptions);
                File.WriteAllText(path, json);

                return path;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba ukládání:\n{ex.Message}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
