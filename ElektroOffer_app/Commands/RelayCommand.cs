﻿using System;
using System.Windows.Input;

namespace ElektroOffer_app.Commands
{
    // =========================================================================
    // 🎛 RelayCommand – univerzální příkaz pro WPF / MVVM
    // =========================================================================
    //
    // K čemu slouží:
    // - Implementuje rozhraní ICommand (standardní WPF příkaz)
    // - Umožňuje předat logiku Execute a CanExecute jako delegáty (Action/Func)
    // - Nemusíš vytvářet samostatnou třídu pro každý příkaz
    //
    // Kde se používá v projektu:
    // - V MainWindow pro klávesové zkratky (Ctrl+S, Ctrl+O, …)
    // - Může se použít i ve ViewModelech (pokud je přidáš později)
    //
    // Jak funguje:
    // - Konstruktor dostane:
    //     - execute(object?) → co se má stát při spuštění příkazu
    //     - canExecute(object?) → kdy je příkaz povolen (např. tlačítko aktivní)
    // - WPF volá CanExecute, aby zjistilo, zda má být tlačítko povolené
    // - WPF volá Execute, když uživatel klikne nebo stiskne klávesovou zkratku
    // =========================================================================
    public class RelayCommand : ICommand
    {
        // ---------------------------------------------------------------------
        // Delegáty – uložená logika příkazu
        // ---------------------------------------------------------------------
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        /// <summary>
        /// Vytvoří nový příkaz.
        /// </summary>
        /// <param name="execute">
        /// Akce, která se provede při Execute().
        /// Nesmí být null – jinak ArgumentNullException.
        /// </param>
        /// <param name="canExecute">
        /// Funkce, která vrací true/false podle toho, zda je příkaz povolen.
        /// Pokud je null → příkaz je vždy povolen.
        /// </param>
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Určuje, zda je příkaz aktuálně povolen.
        /// WPF tuto metodu volá automaticky (např. při změně fokusu, dat apod.).
        /// </summary>
        /// <param name="parameter">Parametr předaný z UI (většinou se nepoužívá).</param>
        public bool CanExecute(object? parameter)
            => _canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// Provede logiku příkazu.
        /// </summary>
        /// <param name="parameter">Parametr předaný z UI (např. CommandParameter).</param>
        public void Execute(object? parameter)
            => _execute(parameter);

        /// <summary>
        /// Událost, kterou WPF sleduje pro znovuvyhodnocení CanExecute.
        /// Když se vyvolá, WPF znovu zavolá CanExecute a podle toho povolí/zakáže tlačítka.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Vyvolá událost CanExecuteChanged.
        /// Použij, pokud se změnily podmínky, za kterých je příkaz povolen.
        /// </summary>
        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
