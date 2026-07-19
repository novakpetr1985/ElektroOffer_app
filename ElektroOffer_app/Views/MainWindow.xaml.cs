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

            // ---------------------------------------------------------
            // 1) Jeden sdílený EF Core kontext pro celou aplikaci
            // ---------------------------------------------------------
            var db = new AppDbContext();
            DatabaseBootstrapService.EnsureReady(db);

            // ---------------------------------------------------------
            // 2) Služby (DI bez kontejneru)
            // ---------------------------------------------------------
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
            var catalogImportService = new CatalogWorkbookImportService(db);
            var measurementImportService = new MeasurementImportService(db);

            // ---------------------------------------------------------
            // 3) Hlavní ViewModel (nastavíme DataContext JEDNOU)
            // ---------------------------------------------------------
            DataContext = new MainViewModel(
                projectService,
                catalogService,
                priceService,
                db,
                messageService,
                printService,
                applicationService,
                windowService,
                catalogImportService,
                fileDialogService,
                messageBoxService,
                measurementImportService);
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
    }
}
