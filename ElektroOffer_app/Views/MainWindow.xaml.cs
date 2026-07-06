using System;
using System.IO;
using System.Windows;
using ElektroOffer_app.Data;
using ElektroOffer_app.Services;
using ElektroOffer_app.Services.Implementations;
using ElektroOffer_app.ViewModels;

namespace ElektroOffer_app
{
    public partial class MainWindow : Window
    {
        // ---------------------------------------------------------
        // Pole třídy pro sdílený EF Core kontext.
        //
        // PROČ JE TO TEĎ POLE, NE JEN LOKÁLNÍ PROMĚNNÁ V KONSTRUKTORU:
        // - Dřív žilo "db" jen jako lokální proměnná uvnitř konstruktoru,
        //   což stačilo, dokud ho potřeboval jen MainViewModel (předaný
        //   rovnou při vytvoření).
        // - Teď ho ale potřebuje i obslužná metoda tlačítka pro
        //   jednorázový import (BtnImportMaterialu_Click), která se
        //   spouští AŽ PO doběhnutí konstruktoru - lokální proměnná by
        //   v tu chvíli už nebyla dostupná (mimo svůj rozsah platnosti).
        //   Uložením do pole třídy (_db) zůstává dostupná po celou dobu
        //   života okna.
        // ---------------------------------------------------------
        private readonly AppDbContext _db;

        public MainWindow()
        {
            InitializeComponent();

            // ---------------------------------------------------------
            // 1) Jeden sdílený EF Core kontext pro celou aplikaci
            // ---------------------------------------------------------
            _db = new AppDbContext();

            // ---------------------------------------------------------
            // 2) Služby (DI bez kontejneru)
            // ---------------------------------------------------------
            var fileDialogService = new RealFileDialogService();
            var fileSystemService = new RealFileSystemService();
            var messageBoxService = new RealMessageBoxService();

            var projectService = new ProjectService(fileDialogService, fileSystemService, messageBoxService);
            var catalogService = new CatalogService();
            var cascadeService = new CalculationCascadeService(_db);
            var priceService = new CalculationPriceService();

            var messageService = new MessageService();
            var printService = new PrintService();
            var applicationService = new ApplicationService();
            var windowService = new WindowService();

            // ---------------------------------------------------------
            // 3) Hlavní ViewModel (nastavíme DataContext JEDNOU)
            // ---------------------------------------------------------
            DataContext = new MainViewModel(
                projectService,
                catalogService,
                cascadeService,
                priceService,
                _db,
                messageService,
                printService,
                applicationService,
                windowService);
        }

        // ---------------------------------------------------------
        // 4) Volitelné: kontrola při zavírání okna
        // ---------------------------------------------------------
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (DataContext is MainViewModel vm)
            {
                if (!vm.CanClose())
                {
                    e.Cancel = true;
                }
            }
        }

        // ===========================================================
        // 5) 🚧 DOČASNÉ: Jednorázový import ceníku materiálu z Excelu
        // ===========================================================
        //
        // K ČEMU SLOUŽÍ:
        // - Naplní tabulky Materials, Categories, Suppliers a
        //   MaterialPrices daty z CSV souboru vyexportovaného z Excel
        //   listu Import_Master (viz Import, ImportCsvReader a
        //   MaterialImportService ve složce Services/DataImport)
        //
        // PROČ JE TO ZA TLAČÍTKEM, NE AUTOMATICKY PŘI STARTU:
        // - Import se spustí JEN po ručním kliknutí, appka ho tedy
        //   NEPROVÁDÍ automaticky při každém spuštění/rebuildu ve VS.
        //   Kdyby byl navázaný na start okna (např. přes Loaded nebo
        //   Dispatcher.InvokeAsync), spouštěl by se úplně zbytečně
        //   při každém debugování, i když už dávno proběhl.
        //
        // PROČ JE BEZPEČNÉ HO PŘESTO SPOUSTĚT I VÍCKRÁT:
        // - MaterialImportService uvnitř používá upsert logiku - podle
        //   dvojice (SupplierId, SupplierCode) buď najde existující
        //   záznam MaterialPrice a jen aktualizuje cenu, nebo založí
        //   nový. Opakované spuštění tedy nevytváří duplicity, jen
        //   zbytečně přepisuje stejná data stejnými hodnotami.
        //
        // "#if DEBUG":
        // - Zajišťuje, že se tlačítko a celá tahle metoda VŮBEC
        //   nezkompilují do ostrého (Release) buildu aplikace - i kdyby
        //   se na smazání tlačítka před vydáním zapomnělo, uživatelům
        //   se nikdy nezobrazí.
        //
        // AŽ IMPORT JEDNOU PROBĚHNE A OVĚŘÍŠ VÝSLEDEK V DATABÁZI:
        // - Smaž tento celý blok (i s "#if DEBUG"/"#endif")
        // - Smaž odpovídající tlačítko z MainWindow.xaml
        // ===========================================================
#if DEBUG
        private async void BtnImportMaterialu_Click(object sender, RoutedEventArgs e)
        {
            // Absolutní cesta k CSV souboru - kořenová složka projektu
            // na tvém disku (D:\_GitHub\ElektroOffer_app\DB\Import).
            // Používáme absolutní cestu záměrně, ne relativní - appka
            // spuštěná přes F5 ve Visual Studiu běží s pracovním
            // adresářem nastaveným na výstupní složku buildu
            // (bin\Debug\...), ne na kořen projektu, takže by relativní
            // cesta typu "DB\Import\import_master.csv" soubor nenašla.
            var cesta = @"D:\_GitHub\ElektroOffer_app\DB\Import\import_master.csv";

            // Kontrola existence souboru PŘED pokusem o čtení -
            // ať dostaneme srozumitelnou hlášku místo neošetřené
            // výjimky FileNotFoundException
            if (!File.Exists(cesta))
            {
                MessageBox.Show($"Soubor nenalezen: {cesta}");
                return;
            }

            var radky = ElektroOffer_app.Services.DataImport.ImportCsvReader.NactiImportCsv(cesta);
            var importService = new ElektroOffer_app.Services.DataImport.MaterialImportService(_db);

            await importService.ImportujMaterialyAsync(radky);

            MessageBox.Show($"Import dokončen – naimportováno {radky.Count} položek.");
        }
#endif
    }
}