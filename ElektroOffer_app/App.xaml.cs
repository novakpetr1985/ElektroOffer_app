﻿using System.Windows;

namespace ElektroOffer_app
{
    /// <summary>
    /// Třída App reprezentuje běžící WPF aplikaci.
    /// 
    /// - Odpovídá deklaraci v App.xaml (x:Class="ElektroOffer_app.App")
    /// - Můžeš zde reagovat na globální události aplikace:
    ///   - OnStartup
    ///   - OnExit
    ///   - zpracování neodchycených výjimek apod.
    /// 
    /// Aktuálně je prázdná → používá se výchozí chování WPF.
    /// </summary>
    public partial class App : Application
    {
        // TODO (pokud bude potřeba):
        // - Přidat globální logování chyb
        // - Přidat načtení konfigurace při startu
        // - Přidat globální DI kontejner (pokud bys šel cestou MVVM/Services)
    }
}
