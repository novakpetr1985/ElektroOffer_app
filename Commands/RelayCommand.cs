using System;
using System.Windows.Input;

namespace ElektroOffer_app.Commands
{
    // =========================================================
    // 🎛 RELAY COMMAND
    // =========================================================
    // 👉 Obecná implementace ICommand pro MVVM
    // 👉 Umožňuje předat logiku Execute/CanExecute jako delegáty
    // 👉 Používá se ve ViewModelech pro binding na tlačítka
    // =========================================================
    public class RelayCommand : ICommand
    {
        // =========================================================
        // 🔧 DELEGÁTY
        // =========================================================
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        // =========================================================
        // 🧩 KONSTRUKTOR
        // =========================================================
        // 👉 execute     - akce, která se má provést
        // 👉 canExecute  - funkce, která určuje, zda je příkaz povolen
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
        // =========================================================
        public bool CanExecute(object? parameter)
            => _canExecute?.Invoke(parameter) ?? true;

        // =========================================================
        // ▶ EXECUTE
        // =========================================================
        // 👉 Spustí předanou akci
        // =========================================================
        public void Execute(object? parameter)
            => _execute(parameter);

        // =========================================================
        // 🔔 CANEXECUTECHANGED
        // =========================================================
        // 👉 Událost pro WPF, když se změní stav CanExecute
        // 👉 Lze vyvolat ručně přes RaiseCanExecuteChanged()
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
