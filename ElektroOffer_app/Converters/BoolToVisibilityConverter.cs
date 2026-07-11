using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ElektroOffer_app.Converters
{
    // ============================================================================
    // 👁 BoolToVisibilityConverter – bool → Visibility pro podmíněné UI prvky
    // ----------------------------------------------------------------------------
    // Účel:
    //   • Umožňuje v XAML skrýt/zobrazit prvek na základě bool property.
    //   • true  → Visibility.Visible
    //   • false → Visibility.Collapsed (ne Hidden – Collapsed prvek nezabírá
    //     místo v layoutu, což je pro formulář žádoucí)
    //
    // Použití (příklad z SupplierSettingsWindow):
    //   Visibility="{Binding IsVatPayer, Converter={StaticResource BoolToVisibilityConverter}}"
    // ============================================================================
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility v && v == Visibility.Visible;
        }
    }
}