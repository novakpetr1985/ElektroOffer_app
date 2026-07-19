using System.Windows;
using ElektroOffer_app.Models;

namespace ElektroOffer_app.Views;

public partial class MeasurementImportWindow : Window
{
    private readonly MeasurementImportPreview _preview;

    public MeasurementImportWindow(MeasurementImportPreview preview)
    {
        InitializeComponent();
        _preview = preview;
        DataContext = preview;
    }

    private void SelectResolved_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in _preview.Items)
            item.IsSelected = item.CanImport;
    }

    private void ClearSelection_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in _preview.Items)
            item.IsSelected = false;
    }

    private void Import_Click(object sender, RoutedEventArgs e)
    {
        if (!_preview.Items.Any(item => item.IsSelected) && !_preview.Attachments.Any(item => item.IsSelected))
        {
            MessageBox.Show("Vyberte alespoň jednu položku nebo přílohu.", "Import terénního měření", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        DialogResult = true;
    }
}
