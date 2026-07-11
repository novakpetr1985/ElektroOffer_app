﻿using System.Windows;
using QuestPDF.Infrastructure;   // 📄 QuestPDF – kvůli nastavení licence

namespace ElektroOffer_app
{
    /// <summary>
    /// App – vstupní bod WPF aplikace
    ///
    /// 🧠 ÚČEL:
    /// - reprezentuje běžící WPF aplikaci (odpovídá x:Class v App.xaml)
    /// - zde se provádí globální inicializace aplikace
    /// - ideální místo pro:
    ///     • nastavení QuestPDF licence
    ///     • globální logování
    ///     • DI kontejner (pokud by se někdy přesouval sem)
    ///     • zpracování neodchycených výjimek
    ///
    /// ✔ AKTUÁLNÍ STAV:
    /// - přidáno nastavení QuestPDF licence (nutné pro generování PDF)
    /// - ostatní logika je ponechána výchozí
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// OnStartup – volá se při startu aplikace
        ///
        /// 🔧 ZDE PROBÍHÁ:
        /// - inicializace QuestPDF licence (nutné, jinak vyhodí výjimku)
        /// - případná budoucí inicializace služeb
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // =========================================================
            // 📄 QUESTPDF – LICENCE
            // =========================================================
            //
            // QuestPDF vyžaduje deklaraci licence před prvním generováním PDF.
            // Bez tohoto řádku vyhodí výjimku:
            //   "Please configure the QuestPDF license..."
            //
            // ✔ Community – zdarma pro jednotlivce, malé firmy, open‑source
            // ✔ Evaluation – pouze pro testování (NE do produkce)
            //
            // ElektroOffer spadá do Community licence.
            //
            QuestPDF.Settings.License = LicenseType.Community;

            // =========================================================
            // 🔧 BUDOUCÍ ROZŠÍŘENÍ (volitelné)
            // =========================================================
            //
            // - Globální logování chyb (AppDomain.CurrentDomain.UnhandledException)
            // - Načtení konfigurace aplikace
            // - Inicializace DI kontejneru (pokud by se přesouval sem)
            // - Registrace globálních služeb
            //
        }

        /// <summary>
        /// OnExit – volá se při ukončení aplikace
        ///
        /// 🔧 Můžeš zde:
        /// - uklidit zdroje
        /// - zavřít DB spojení
        /// - uložit stav aplikace
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // TODO: případné čištění zdrojů
        }
    }
}
