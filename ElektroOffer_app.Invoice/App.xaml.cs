using System.Windows;
using ElektroOffer_app.Invoice.Views;

namespace ElektroOffer_app.Invoice
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var window = new InvoiceWindow([]);
            window.Show();
        }
    }
}
