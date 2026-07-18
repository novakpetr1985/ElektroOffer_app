using System.ComponentModel;
using System.Runtime.CompilerServices;
using ElektroOffer_app.Services;

namespace ElektroOffer_app.ViewModels
{
    /// <summary>Zpřístupňuje volbu motivu nastavení a oznamuje změny všech přepínačů UI.</summary>
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly AppThemeService _themeService;

        public SettingsViewModel(AppThemeService themeService)
        {
            _themeService = themeService;
        }

        public bool IsSystemTheme
        {
            get => _themeService.CurrentMode == AppThemeMode.System;
            set { if (value) SetTheme(AppThemeMode.System); }
        }

        public bool IsLightTheme
        {
            get => _themeService.CurrentMode == AppThemeMode.Light;
            set { if (value) SetTheme(AppThemeMode.Light); }
        }

        public bool IsDarkTheme
        {
            get => _themeService.CurrentMode == AppThemeMode.Dark;
            set { if (value) SetTheme(AppThemeMode.Dark); }
        }

        private void SetTheme(AppThemeMode mode)
        {
            _themeService.SetMode(mode);
            OnPropertyChanged(nameof(IsSystemTheme));
            OnPropertyChanged(nameof(IsLightTheme));
            OnPropertyChanged(nameof(IsDarkTheme));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
