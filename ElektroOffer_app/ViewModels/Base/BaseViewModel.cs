using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElektroOffer_app.ViewModels.Base
{
    // =========================================================
    // 🧠 BASE VIEWMODEL
    // =========================================================
    // 👉 Základní třída pro všechny ViewModely
    // 👉 Implementuje INotifyPropertyChanged
    // 👉 Poskytuje SetProperty a OnPropertyChanged
    // =========================================================
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
