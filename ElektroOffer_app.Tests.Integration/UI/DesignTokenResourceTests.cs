using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Integration.UI;

[TestFixture]
[Apartment(ApartmentState.STA)]
public class DesignTokenResourceTests
{
    [OneTimeSetUp]
    public void InitializeWpfApplication()
    {
        _ = Application.Current ?? new Application();
    }

    [TestCase("/ElektroOffer_app;component/Resources/Colors.xaml",
              "/ElektroOffer_app;component/Resources/Styles.xaml")]
    [TestCase("/ElektroOffer_app.Invoice;component/Resources/Colors.xaml",
              "/ElektroOffer_app.Invoice;component/Resources/Styles.xaml")]
    public void ResourceDictionaries_Should_Apply_GroupBox_Style_Without_XamlParseException(
        string colorsUri,
        string stylesUri)
    {
        var resources = new ResourceDictionary();
        resources.MergedDictionaries.Add(Load("/ElektroOffer_app.Invoice;component/Resources/DesignTokens.xaml"));
        resources.MergedDictionaries.Add(Load(colorsUri));
        resources.MergedDictionaries.Add(Load(stylesUri));

        var groupBox = new GroupBox
        {
            Resources = resources,
            Style = (Style)resources[typeof(GroupBox)]
        };

        Assert.DoesNotThrow(() =>
        {
            groupBox.ApplyTemplate();
            groupBox.Measure(new Size(320, 200));
        });
        Assert.That(groupBox.Padding, Is.EqualTo(new Thickness(8)));
    }

    [TestCase("/ElektroOffer_app;component/Resources/Colors.xaml",
              "/ElektroOffer_app;component/Resources/Styles.xaml")]
    [TestCase("/ElektroOffer_app.Invoice;component/Resources/Colors.xaml",
              "/ElektroOffer_app.Invoice;component/Resources/Styles.xaml")]
    public void Every_Implicit_Control_Style_Should_Be_Runtime_Applicable(
        string colorsUri,
        string stylesUri)
    {
        var resources = new ResourceDictionary();
        resources.MergedDictionaries.Add(Load("/ElektroOffer_app.Invoice;component/Resources/DesignTokens.xaml"));
        resources.MergedDictionaries.Add(Load(colorsUri));
        resources.MergedDictionaries.Add(Load(stylesUri));

        var controlTypes = new[]
        {
            typeof(Window), typeof(Menu), typeof(MenuItem), typeof(GroupBox), typeof(StatusBar),
            typeof(ToolBarTray), typeof(ToolBar), typeof(Button), typeof(TextBox),
            typeof(ComboBox), typeof(ComboBoxItem), typeof(ListBoxItem), typeof(ListView),
            typeof(ListViewItem), typeof(GridViewColumnHeader), typeof(CheckBox),
            typeof(RadioButton), typeof(DataGrid), typeof(DataGridRow),
            typeof(DataGridColumnHeader), typeof(DataGridCell), typeof(TextBlock)
        };

        Assert.Multiple(() =>
        {
            foreach (var type in controlTypes)
            {
                if (resources[type] is not Style style)
                    continue;

                Assert.DoesNotThrow(() =>
                {
                    var element = (FrameworkElement)Activator.CreateInstance(type)!;
                    element.Resources = resources;
                    element.Style = style;
                    element.ApplyTemplate();
                    element.Measure(new Size(320, 200));
                }, $"Implicit style for {type.Name} cannot be applied.");
            }
        });
    }

    [Test]
    public void DesignTokens_Should_Have_Wpf_Compatible_Types()
    {
        var tokens = Load("/ElektroOffer_app.Invoice;component/Resources/DesignTokens.xaml");

        Assert.Multiple(() =>
        {
            Assert.That(tokens["Space.2"], Is.TypeOf<double>());
            Assert.That(tokens["Spacing.GroupBoxPadding"], Is.TypeOf<Thickness>());
            Assert.That(tokens["Spacing.ControlPadding"], Is.TypeOf<Thickness>());
            Assert.That(tokens["Typography.FontSize.Body"], Is.TypeOf<double>());
        });
    }

    private static ResourceDictionary Load(string uri)
        => new() { Source = new Uri(uri, UriKind.Relative) };
}
