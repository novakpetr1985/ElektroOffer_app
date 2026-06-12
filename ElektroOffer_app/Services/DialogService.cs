﻿using System.Windows;

namespace ElektroOffer_app.Services
{
    // ========================================================================
    // HLAVNÍ: DialogService – jednoduchý wrapper pro MessageBox
    // ========================================================================
    //
    // ÚČEL:
    // - Umožňuje ViewModelům zobrazovat dialogy bez přímé závislosti na UI.
    // - Odděluje prezentační logiku od aplikační logiky.
    //
    // POUŽITÍ:
    // - ViewModel zavolá DialogService.ShowInfo / ShowWarning / ShowError / Confirm.
    // - UI (MessageBox) se zobrazí, ale ViewModel o něm nic neví.
    //
    // POZNÁMKA:
    // - V MVVM se MessageBox nepoužívá přímo ve ViewModelu.
    // - Tato služba je kompromisní řešení pro menší projekty.
    // ========================================================================
    public class DialogService
    {
        // --------------------------------------------------------------------
        // VEDLEJŠÍ: Informační dialog
        // --------------------------------------------------------------------
        /// <summary>
        /// Zobrazí informační dialog s tlačítkem OK.
        /// </summary>
        public void ShowInfo(string message, string title = "Informace")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // --------------------------------------------------------------------
        // VEDLEJŠÍ: Varovný dialog
        // --------------------------------------------------------------------
        /// <summary>
        /// Zobrazí varovný dialog s tlačítkem OK.
        /// </summary>
        public void ShowWarning(string message, string title = "Upozornění")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // --------------------------------------------------------------------
        // VEDLEJŠÍ: Chybový dialog
        // --------------------------------------------------------------------
        /// <summary>
        /// Zobrazí chybový dialog s tlačítkem OK.
        /// </summary>
        public void ShowError(string message, string title = "Chyba")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // --------------------------------------------------------------------
        // HLAVNÍ: Potvrzovací dialog (Ano/Ne)
        // --------------------------------------------------------------------
        /// <summary>
        /// Zobrazí potvrzovací dialog s tlačítky Ano/Ne.
        /// Vrací true = Ano, false = Ne.
        /// </summary>
        public bool Confirm(string message, string title = "Potvrzení")
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }
    }
}
