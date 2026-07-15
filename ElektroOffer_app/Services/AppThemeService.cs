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
                SetBrush(resources, "AppBackgroundBrush", "#202020");
                SetBrush(resources, "AppSurfaceBrush", "#2D2D30");
                SetBrush(resources, "AppSurfaceAltBrush", "#3A3A3D");
                SetBrush(resources, "AppBorderBrush", "#555555");
                SetBrush(resources, "AppTextBrush", "#F3F3F3");
                SetBrush(resources, "AppMutedTextBrush", "#C8C8C8");
                SetBrush(resources, "AppInputBrush", "#FFFFFF");
                SetBrush(resources, "AppAccentBrush", "#1E5AA8");
                SetBrush(resources, "AppAccentHoverBrush", "#164A8A");
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
                SetBrush(resources, "AppAccentBrush", "#2563EB");
                SetBrush(resources, "AppAccentHoverBrush", "#1D4ED8");
                SetBrush(resources, "AppDangerBrush", "#DC2626");
                SetBrush(resources, "AppSuccessBrush", "#15803D");
                SetBrush(resources, "AppWarningBrush", "#B91C1C");
            }
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

        private static void SetBrush(ResourceDictionary resources, string key, string color)
            => resources[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));

        private sealed record ThemeSettings(string ThemeMode);
    }
}
