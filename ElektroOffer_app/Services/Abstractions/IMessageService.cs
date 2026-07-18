// IMessageService.cs
// -------------------------------------------------------------
// Abstrakce pro zobrazování zpráv (MessageBox).
// ViewModel nesmí volat MessageBox přímo – proto tato služba.
// -------------------------------------------------------------

using System.Windows;

namespace ElektroOffer_app.Services
{
    /// <summary>
    /// Poskytuje obecné potvrzovací dialogy používané hlavním ViewModelem.
    /// </summary>
    public interface IMessageService
    {
        // Zobrazí dialog Ano/Ne a vrátí true, pokud uživatel zvolí Ano.
        bool ShowYesNo(string message, string title);

        // Zobrazí dialog Ano/Ne/Storno a vrátí výsledek.
        MessageBoxResult ShowYesNoCancel(string message, string title);
    }
}
