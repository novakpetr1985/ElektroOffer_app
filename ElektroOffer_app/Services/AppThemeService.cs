using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace ElektroOffer_app.Services
{
    public enum AppThemeMode
    {
        System,
        Light,
        Dark
    }

    public class AppThemeService
    {
        // Uživatelská volba motivu se ukládá mimo projektový soubor,
        // aby platila pro celou aplikaci a všechny otevírané projekty.
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ElektroOffer",
            "settings.json");

        public AppThemeMode CurrentMode { get; private set; } = AppThemeMode.System;

        public void LoadAndApply()
        {
            CurrentMode = LoadMode();
            Apply(CurrentMode);
        }

        public void SetMode(AppThemeMode mode)
        {
            CurrentMode = mode;
            SaveMode(mode);
            Apply(mode);
        }

        private static AppThemeMode LoadMode()
        {
            if (!File.Exists(SettingsPath))
                return AppThemeMode.System;

            try
            {
                var json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<ThemeSettings>(json);
                return Enum.TryParse<AppThemeMode>(settings?.ThemeMode, out var mode)
                    ? mode
                    : AppThemeMode.System;
            }
            catch
            {
                return AppThemeMode.System;
            }
        }

        private static void SaveMode(AppThemeMode mode)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            var json = JsonSerializer.Serialize(new ThemeSettings(mode.ToString()), new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(SettingsPath, json);
        }

        private static void Apply(AppThemeMode mode)
        {
            var useDark = mode == AppThemeMode.Dark ||
                          mode == AppThemeMode.System && IsWindowsDarkMode();

            var resources = Application.Current.Resources;

            if (useDark)
            {
                var useWindows11Palette = IsWindows11OrNewer();

                SetBrush(resources, "AppBackgroundBrush", useWindows11Palette ? "#202020" : "#1F1F1F");
                SetBrush(resources, "AppSurfaceBrush", useWindows11Palette ? "#2B2B2B" : "#2D2D30");
                SetBrush(resources, "AppSurfaceAltBrush", useWindows11Palette ? "#383838" : "#3A3A3D");
                SetBrush(resources, "AppBorderBrush", useWindows11Palette ? "#5A5A5A" : "#555555");
                SetBrush(resources, "AppTextBrush", "#F3F3F3");
                SetBrush(resources, "AppMutedTextBrush", "#C8C8C8");
                SetBrush(resources, "AppInputBrush", useWindows11Palette ? "#242424" : "#252526");
                SetBrush(resources, "AppInputTextBrush", "#F3F3F3");
                SetBrush(resources, "AppDisabledInputBrush", useWindows11Palette ? "#404040" : "#3F3F46");
                SetBrush(resources, "AppDisabledTextBrush", "#D1D5DB");
                SetBrush(resources, "AppButtonBrush", useWindows11Palette ? "#3A3A3A" : "#3A3A3D");
                SetBrush(resources, "AppAccentBrush", "#1E5AA8");
                SetBrush(resources, "AppAccentHoverBrush", "#164A8A");
                SetBrush(resources, "AppSelectionBrush", "#2B78D4");
                SetBrush(resources, "AppSelectionTextBrush", "#FFFFFF");
                SetBrush(resources, "AppDangerBrush", "#B3261E");
                SetBrush(resources, "AppSuccessBrush", "#57A64A");
                SetBrush(resources, "AppWarningBrush", "#F1707A");
            }
            else
            {
                SetBrush(resources, "AppBackgroundBrush", "#F4F6F8");
                SetBrush(resources, "AppSurfaceBrush", "#FFFFFF");
                SetBrush(resources, "AppSurfaceAltBrush", "#EEF2F6");
                SetBrush(resources, "AppBorderBrush", "#D6DDE6");
                SetBrush(resources, "AppTextBrush", "#1F2937");
                SetBrush(resources, "AppMutedTextBrush", "#667085");
                SetBrush(resources, "AppInputBrush", "#FFFFFF");
                SetBrush(resources, "AppInputTextBrush", "#1F2937");
                SetBrush(resources, "AppDisabledInputBrush", "#EEF2F6");
                SetBrush(resources, "AppDisabledTextBrush", "#7B8794");
                SetBrush(resources, "AppButtonBrush", "#F3F4F6");
                SetBrush(resources, "AppAccentBrush", "#2563EB");
                SetBrush(resources, "AppAccentHoverBrush", "#1D4ED8");
                SetBrush(resources, "AppSelectionBrush", "#2563EB");
                SetBrush(resources, "AppSelectionTextBrush", "#FFFFFF");
                SetBrush(resources, "AppDangerBrush", "#DC2626");
                SetBrush(resources, "AppSuccessBrush", "#15803D");
                SetBrush(resources, "AppWarningBrush", "#B91C1C");
            }

            ApplySystemBrushes(resources);
        }

        private static void ApplySystemBrushes(ResourceDictionary resources)
        {
            // Některé standardní WPF šablony čtou přímo SystemColors.
            // Přemapování drží čitelný výběr, menu a vstupy i v tmavém režimu.
            resources[SystemColors.WindowBrushKey] = resources["AppBackgroundBrush"];
            resources[SystemColors.WindowTextBrushKey] = resources["AppTextBrush"];
            resources[SystemColors.ControlBrushKey] = resources["AppSurfaceBrush"];
            resources[SystemColors.ControlTextBrushKey] = resources["AppTextBrush"];
            resources[SystemColors.ControlLightBrushKey] = resources["AppSurfaceAltBrush"];
            resources[SystemColors.ControlDarkBrushKey] = resources["AppBorderBrush"];
            resources[SystemColors.MenuBrushKey] = resources["AppSurfaceBrush"];
            resources[SystemColors.MenuTextBrushKey] = resources["AppTextBrush"];
            resources[SystemColors.HighlightBrushKey] = resources["AppSelectionBrush"];
            resources[SystemColors.HighlightTextBrushKey] = resources["AppSelectionTextBrush"];
            resources[SystemColors.InactiveSelectionHighlightBrushKey] = resources["AppSelectionBrush"];
            resources[SystemColors.InactiveSelectionHighlightTextBrushKey] = resources["AppSelectionTextBrush"];
            resources[SystemColors.HotTrackBrushKey] = resources["AppSelectionBrush"];
            resources[SystemColors.GrayTextBrushKey] = resources["AppMutedTextBrush"];
        }

        private static bool IsWindowsDarkMode()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var value = key?.GetValue("AppsUseLightTheme");
                return value is int intValue && intValue == 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsWindows11OrNewer()
            => OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000);

        private static void SetBrush(ResourceDictionary resources, string key, string color)
            => resources[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));

        private sealed record ThemeSettings(string ThemeMode);
    }
}
