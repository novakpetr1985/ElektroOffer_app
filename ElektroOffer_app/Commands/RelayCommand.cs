﻿using System;
using System.Windows.Input;

namespace ElektroOffer_app.Commands
{
    // ============================================================================
    // 🎛 RELAY COMMAND (ICommand) - univerzální příkaz pro WPF / MVVM
    // ============================================================================
    //
    // Univerzální implementace příkazu pro WPF / MVVM.
    //
    // Proč existuje:
    //   - WPF používá ICommand pro tlačítka, menu, klávesové zkratky.
    //   - RelayCommand umožňuje předat logiku příkazu jako delegát (Action/Func),
    //     takže nemusíš vytvářet samostatné třídy pro každý příkaz.
    //
    // Kde se používá:
    //   - V MainViewModelu pro všechny příkazy (AddWorkItem, AddMaterialItem, Save, Load…)
    //   - V MainWindow pro klávesové zkratky (Ctrl+S, Ctrl+O…)
    //
    // Jak funguje:
    //   - Execute(object?) → provede akci
    //   - CanExecute(object?) → určí, zda je příkaz povolen (tlačítko aktivní)
    //   - RaiseCanExecuteChanged() → oznámí WPF, že se má znovu vyhodnotit CanExecute
    //
    // ============================================================================
    /// <summary>Propojuje akci a podmínku její dostupnosti s WPF rozhraním <see cref="ICommand"/>.</summary>
    public class RelayCommand : ICommand
    {
        // ------------------------------------------------------------------------
        // 🔧 Delegáty – uložená logika příkazu
        // ------------------------------------------------------------------------
        //
        // _execute:
        //   - Metoda, která se má provést při Execute().
        //   - Action<object?> → může přijmout parametr z CommandParameter.
        //
        // _canExecute:
        //   - Funkce, která vrací true/false podle toho, zda je příkaz povolen.
        //   - Pokud je null → příkaz je vždy povolen.
        //
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        // ------------------------------------------------------------------------
        // 🧩 Konstruktor
        // ------------------------------------------------------------------------
        //
        // Příklad použití:
        //   SaveCommand = new RelayCommand(_ => SaveProject());
        //   DeleteItemCommand = new RelayCommand(_ => DeleteItem(), _ => CanDeleteItem());
        //
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            // Execute je povinné – bez něj příkaz nedává smysl
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));

            // CanExecute je volitelné – pokud není, příkaz je vždy povolen
            _canExecute = canExecute;
        }

        // ------------------------------------------------------------------------
        // 🔒 CanExecute – zda je příkaz povolen
        // ------------------------------------------------------------------------
        //
        // WPF tuto metodu volá automaticky:
        //   - při změně fokusu
        //   - při změně dat
        //   - při volání RaiseCanExecuteChanged()
        //
        public bool CanExecute(object? parameter)
            => _canExecute?.Invoke(parameter) ?? true;

        // ------------------------------------------------------------------------
        // ▶ Execute – provedení příkazu
        // ------------------------------------------------------------------------
        //
        // WPF zavolá tuto metodu, když:
        //   - uživatel klikne na tlačítko
        //   - stiskne klávesovou zkratku
        //   - vyvolá se CommandBinding
        //
        public void Execute(object? parameter)
            => _execute(parameter);

        // ------------------------------------------------------------------------
        // 🔔 CanExecuteChanged – událost sledovaná WPF
        // ------------------------------------------------------------------------
        //
        // WPF se na tuto událost přihlásí a když se vyvolá,
        // znovu zavolá CanExecute() → povolí/zakáže tlačítka.
        //
        public event EventHandler? CanExecuteChanged;

        // ------------------------------------------------------------------------
        // 🔄 RaiseCanExecuteChanged – ruční vyvolání události
        // ------------------------------------------------------------------------
        //
        // Používá se, když se změní podmínky, které ovlivňují CanExecute.
        //
        // Příklad:
        //   - tlačítko „Smazat“ má být aktivní jen když je vybraná položka
        //   - po změně výběru zavoláš DeleteCommand.RaiseCanExecuteChanged()
        //
        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
