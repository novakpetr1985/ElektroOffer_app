using System.Windows;
using ElektroOffer_app.Data;
using ElektroOffer_app.Services;
using ElektroOffer_app.Services.Implementations;
using ElektroOffer_app.ViewModels;

namespace ElektroOffer_app
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // ============================================================
            // 1) Jeden sdílený EF Core kontext pro celou aplikaci
            // ------------------------------------------------------------
            // • DbContext se vytváří JEDNOU při startu aplikace.
            // • Používá se ve všech službách (CascadeService, ProjectService).
            // ============================================================
            var db = new AppDbContext();

            // ============================================================
            // 2) Služby (DI bez kontejneru)
            // ------------------------------------------------------------
            // • Reálné implementace pro dialogy, filesystem, messagebox.
            // • ProjectService používá tyto služby + DbContext.
            // ============================================================
            var fileDialogService = new RealFileDialogService();
            var fileSystemService = new RealFileSystemService();
            var messageBoxService = new RealMessageBoxService();

            var projectService = new ProjectService(fileDialogService, fileSystemService, messageBoxService);
            var catalogService = new CatalogService();
            var priceService = new CalculationPriceService();

            var messageService = new MessageService();
            var printService = new PrintService();
            var applicationService = new ApplicationService();
            var windowService = new WindowService();

            // ============================================================
            // 3) Hlavní ViewModel (nastavíme DataContext JEDNOU)
            // ------------------------------------------------------------
            // • MainViewModel dostane přesně ty služby, které jeho konstruktor vyžaduje.
            // ============================================================
            DataContext = new MainViewModel(
                projectService,
                catalogService,
                priceService,
                db,
                messageService,
                printService,
                applicationService,
                windowService);

            // ============================================================
            // 4) Načtení dat po otevření okna
            // ------------------------------------------------------------
            // • Zde se načítají Tasks, Specifications, Materials, Prices…
            // • Bez tohoto kroku by UI bylo prázdné (tvůj problém č. 1).
            // ============================================================
            Loaded += MainWindow_Loaded;
        }

        // ============================================================
        // 5) Načtení dat po otevření hlavního okna
        // ------------------------------------------------------------
        // Účel:
        //   • Po startu aplikace se musí načíst statická data z DB:
        //         – Práce (WorkTask, WorkSpecification, BaseMaterial, Position)
        //         – Materiál (Category, ProductName, Supplier, Offer, Price)
        //   • Bez tohoto kroku by UI bylo prázdné (Tasks, Materials…).
        //
        // Proč zde:
        //   • MainWindow je místo, kde se inicializuje DataContext.
        //   • Po jeho vytvoření je ideální okamžik zavolat LoadData().
        //   • ViewModel má plnou kontrolu nad tím, co se načítá.
        // ============================================================
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                // 🔥 Načtení všech statických dat z DB
                vm.LoadData();
            }
        }

        // ============================================================
        // 6) Kontrola při zavírání okna
        // ------------------------------------------------------------
        // • Umožňuje ViewModelu zabránit zavření (např. neuložené změny).
        // ============================================================
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
    }
}
