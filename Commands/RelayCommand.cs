using System;
using System.Windows.Input;

namespace ElektroOffer_app.Commands
{
    // =========================================================
    // 🎛 RELAY COMMAND (UNIVERZÁLNÍ PŘÍKAZ PRO MVVM)
    // =========================================================
    // 👉 Obecná implementace ICommand používaná v MVVM aplikacích
    // 👉 Umožňuje předat logiku Execute a CanExecute jako delegáty
    // 👉 Díky tomu nemusíš vytvářet samostatnou třídu pro každý příkaz
    // 👉 Používá se v KeyBinding (MainWindow) i ve ViewModelech
    // =========================================================
    public class RelayCommand : ICommand
    {
        // =========================================================
        // 🔧 DELEGÁTY
        // =========================================================
        // _execute     → akce, která se má provést při spuštění příkazu
        // _canExecute  → funkce, která určuje, zda je příkaz povolen
        //                (pokud je null → příkaz je vždy povolen)
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        // =========================================================
        // 🧩 KONSTRUKTOR
        // =========================================================
        // 👉 execute     - povinný delegát, který se provede při Execute()
        // 👉 canExecute  - volitelný delegát, který určuje, zda lze příkaz spustit
        // =========================================================
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // =========================================================
        // 🔒 CAN EXECUTE
        // =========================================================
        // 👉 Vrací true/false podle toho, zda je příkaz povolen
        // 👉 Pokud není zadán canExecute delegát → vždy true
        // 👉 WPF volá tuto metodu automaticky při změně UI
        // =========================================================
        public bool CanExecute(object? parameter)
            => _canExecute?.Invoke(parameter) ?? true;

        // =========================================================
        // ▶ EXECUTE
        // =========================================================
        // 👉 Spustí předanou akci (execute delegát)
        // 👉 Používá se při kliknutí na tlačítko nebo při KeyBinding
        // =========================================================
        public void Execute(object? parameter)
            => _execute(parameter);

        // =========================================================
        // 🔔 CANEXECUTECHANGED
        // =========================================================
        // 👉 Událost, kterou WPF sleduje pro povolení/zakázání tlačítek
        // 👉 Pokud se změní podmínky CanExecute → zavolá se RaiseCanExecuteChanged()
        // =========================================================
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Vyvolá událost CanExecuteChanged a tím donutí WPF znovu
        /// vyhodnotit, zda je příkaz povolen (např. povolit/zakázat tlačítko).
        /// </summary>
        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
