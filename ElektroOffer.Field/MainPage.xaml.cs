using System.Globalization;
using ElektroOffer.Contracts.Measurements;
using ElektroOffer.Contracts.Catalog;
using ElektroOffer.Field.Services;

namespace ElektroOffer.Field;

public partial class MainPage : ContentPage
{
    private readonly FieldStorageService _storage = new();
    private readonly FieldCatalogStorageService _catalogStorage = new();
    private MeasurementPackage _package = new() { SourceAppVersion = "1.13.0-feature" };
    private MeasurementArea? _selectedArea;
    private MeasurementItem? _selectedItem;
    private FieldCatalogSnapshot? _catalog;
    private FieldCatalogOption? _selectedCatalogOption;

    public MainPage()
    {
        InitializeComponent();
        KindPicker.ItemsSource = new[]
        {
            new KindOption(MeasurementKind.CableRoute, "Kabelová trasa"),
            new KindOption(MeasurementKind.Socket, "Zásuvka"),
            new KindOption(MeasurementKind.Switch, "Vypínač"),
            new KindOption(MeasurementKind.Light, "Světlo / vývod"),
            new KindOption(MeasurementKind.DistributionBoard, "Rozvaděč"),
            new KindOption(MeasurementKind.Custom, "Jiná položka")
        };
        KindPicker.SelectedIndex = 0;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        Loaded -= OnLoaded;
        try
        {
            _package = await _storage.LoadAsync() ?? _package;
            _catalog = await _catalogStorage.LoadAsync();
            RefreshPage();
            SetStatus(_catalog == null
                ? "Načtěte aktuální katalog z hlavní aplikace"
                : _package.Project.Name.Length > 0 ? "Obnoven poslední lokální koncept" : "Založte zakázku nebo načtěte testovací data");
        }
        catch (Exception ex)
        {
            SetStatus($"Koncept se nepodařilo obnovit: {ex.Message}");
        }
    }

    private async void OnProjectFieldUnfocused(object? sender, FocusEventArgs e) => await SaveProjectFieldsAsync();

    private async Task SaveProjectFieldsAsync()
    {
        ApplyProjectFields();
        await _storage.SaveAsync(_package);
        SetStatus("Koncept automaticky uložen");
    }

    private void ApplyProjectFields()
    {
        _package.Project.Name = ProjectNameEntry.Text?.Trim() ?? string.Empty;
        _package.Project.CustomerName = CustomerEntry.Text?.Trim() ?? string.Empty;
        _package.Project.SiteAddress = AddressEntry.Text?.Trim() ?? string.Empty;
        _package.Project.TechnicianName = TechnicianEntry.Text?.Trim() ?? string.Empty;
        _package.Project.Note = ProjectNoteEditor.Text?.Trim() ?? string.Empty;
    }

    private async void OnNewClicked(object? sender, EventArgs e)
    {
        if (!await DisplayAlertAsync("Nové měření", "Nahradit aktuální lokální koncept novým měřením?", "Ano", "Ne"))
            return;

        _package = new MeasurementPackage
        {
            SourceAppVersion = "1.13.0-feature",
            CatalogVersion = _catalog?.CatalogVersion ?? string.Empty
        };
        _selectedArea = null;
        _selectedItem = null;
        _selectedCatalogOption = null;
        await _storage.SaveAsync(_package);
        RefreshPage();
        SetStatus("Nové měření připraveno");
    }

    private async void OnLoadDemoClicked(object? sender, EventArgs e)
    {
        if (_catalog == null)
        {
            SetStatus("Nejprve tlačítkem Načíst katalog načtěte .eofcatalog z hlavní aplikace");
            return;
        }

        if (!await DisplayAlertAsync("Testovací data", "Nahradit aktuální koncept vzorovým rodinným domem?", "Načíst", "Zrušit"))
            return;

        _package = DemoMeasurementFactory.Create(_catalog);
        _selectedArea = _package.Project.Areas.FirstOrDefault();
        _selectedItem = null;
        _selectedCatalogOption = null;
        await _storage.SaveAsync(_package);
        RefreshPage();
        SetStatus("Testovací data byla načtena a lokálně uložena");
    }

    private async void OnImportCatalogClicked(object? sender, EventArgs e)
    {
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Vyberte katalog .eofcatalog z hlavní aplikace" });
            if (file == null) return;
            _catalog = await _catalogStorage.ImportAsync(file);
            _package.CatalogVersion = _catalog.CatalogVersion;
            await _storage.SaveAsync(_package);
            RefreshCatalog();
            SetStatus($"Katalog {_catalog.CatalogVersion} byl bezpečně načten");
        }
        catch (Exception ex)
        {
            SetStatus($"Katalog nelze načíst: {ex.Message}");
        }
    }

    private void OnCatalogSelected(object? sender, EventArgs e)
    {
        _selectedCatalogOption = (CatalogPicker.SelectedItem as CatalogChoice)?.Option;
        if (_selectedCatalogOption == null) return;
        ItemNameEntry.Text = _selectedCatalogOption.Name;
        UnitEntry.Text = _selectedCatalogOption.Unit;
        var kind = InferKind(_selectedCatalogOption);
        KindPicker.SelectedItem = ((IEnumerable<KindOption>)KindPicker.ItemsSource).First(option => option.Kind == kind);
    }

    private async void OnAddAreaClicked(object? sender, EventArgs e)
    {
        var name = AreaNameEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            SetStatus("Zadejte název místnosti nebo části stavby");
            return;
        }

        var area = new MeasurementArea { Name = name };
        _package.Project.Areas.Add(area);
        _selectedArea = area;
        _selectedItem = null;
        AreaNameEntry.Text = string.Empty;
        await _storage.SaveAsync(_package);
        RefreshAreas();
        SetStatus($"Přidáno: {name}");
    }

    private void OnAreaSelected(object? sender, EventArgs e)
    {
        _selectedArea = AreaPicker.SelectedItem as MeasurementArea;
        _selectedItem = null;
        RefreshItems();
    }

    private async void OnAddItemClicked(object? sender, EventArgs e)
    {
        if (_selectedArea == null)
        {
            SetStatus("Nejprve vyberte místnost");
            return;
        }

        if (_catalog == null || _selectedCatalogOption == null)
        {
            SetStatus("Nejprve načtěte katalog a vyberte pracovní úkon nebo kategorii materiálu");
            return;
        }

        var name = ItemNameEntry.Text?.Trim();
        if (!decimal.TryParse(QuantityEntry.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var quantity)
            || quantity <= 0)
        {
            SetStatus("Vyplňte platné množství větší než nula");
            return;
        }
        name = string.IsNullOrWhiteSpace(name) ? _selectedCatalogOption.Name : name;

        decimal.TryParse(ReserveEntry.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var reserve);
        var kind = (KindPicker.SelectedItem as KindOption)?.Kind ?? MeasurementKind.Custom;
        var item = new MeasurementItem
        {
            Kind = kind,
            DisplayName = name,
            Quantity = quantity,
            Unit = string.IsNullOrWhiteSpace(UnitEntry.Text) ? DefaultUnit(kind) : UnitEntry.Text.Trim(),
            ReservePercent = Math.Clamp(reserve, 0, 100),
            Note = ItemNoteEditor.Text?.Trim() ?? string.Empty,
            CatalogCode = _selectedCatalogOption?.Code ?? string.Empty
        };
        if (_selectedCatalogOption?.Kind == FieldCatalogOptionKind.Work)
        {
            item.WorkHints.Add(new WorkHint
            {
                CatalogCode = _selectedCatalogOption.Code,
                DisplayName = _selectedCatalogOption.Name,
                Quantity = quantity,
                Unit = item.Unit,
                RuleId = "catalog-selection-v1",
                Confidence = 1m
            });
        }
        else if (_selectedCatalogOption?.Kind is FieldCatalogOptionKind.MaterialCategory or FieldCatalogOptionKind.Material)
        {
            item.MaterialRequirements.Add(new MaterialRequirement
            {
                CategoryCode = _selectedCatalogOption.Kind == FieldCatalogOptionKind.MaterialCategory ? _selectedCatalogOption.Code : string.Empty,
                MaterialCode = _selectedCatalogOption.Kind == FieldCatalogOptionKind.Material ? _selectedCatalogOption.Code : string.Empty,
                Category = _selectedCatalogOption.Kind == FieldCatalogOptionKind.MaterialCategory ? _selectedCatalogOption.Name : _selectedCatalogOption.Category,
                Specification = string.Empty,
                Quantity = quantity,
                Unit = item.Unit,
                ReservePercent = item.ReservePercent
            });
        }
        _selectedArea.Items.Add(item);
        _selectedArea.UpdatedAtUtc = DateTime.UtcNow;
        _selectedItem = item;
        ClearItemForm();
        await _storage.SaveAsync(_package);
        RefreshItems();
        SetStatus($"Položka „{name}“ byla přidána");
    }

    private void OnItemSelected(object? sender, SelectionChangedEventArgs e)
    {
        _selectedItem = e.CurrentSelection.FirstOrDefault() as MeasurementItem;
        RefreshPhotoCount();
    }

    private async void OnDeleteItemClicked(object? sender, EventArgs e)
    {
        if (_selectedArea == null || _selectedItem == null)
        {
            SetStatus("K vymazání nejprve označte položku v seznamu");
            return;
        }

        var name = _selectedItem.DisplayName;
        if (!await DisplayAlertAsync("Vymazat položku", $"Opravdu vymazat „{name}“?", "Vymazat", "Zrušit"))
            return;
        _package.Attachments.RemoveAll(photo => photo.ItemId == _selectedItem.Id);
        _selectedArea.Items.Remove(_selectedItem);
        _selectedItem = null;
        await _storage.SaveAsync(_package);
        RefreshItems();
        SetStatus("Položka byla vymazána");
    }

    private async void OnAddPhotoClicked(object? sender, EventArgs e)
    {
        if (_selectedArea == null)
        {
            SetStatus("Nejprve vyberte místnost");
            return;
        }

        try
        {
            var photo = (await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { Title = "Vyberte fotografii ze stavby" })).FirstOrDefault();
            if (photo == null)
                return;
            var reference = await _storage.AddPhotoAsync(photo, _selectedArea.Id, _selectedItem?.Id);
            _package.Attachments.Add(reference);
            await _storage.SaveAsync(_package);
            RefreshPhotoCount();
            SetStatus("Fotografie byla uložena do offline konceptu");
        }
        catch (Exception ex)
        {
            SetStatus($"Fotografii se nepodařilo přidat: {ex.Message}");
        }
    }

    private async void OnExportClicked(object? sender, EventArgs e)
    {
        try
        {
            ApplyProjectFields();
            _package.CatalogVersion = _catalog?.CatalogVersion ?? string.Empty;
            var validation = MeasurementPackageValidator.Validate(_package);
            if (!validation.IsValid)
            {
                await DisplayAlertAsync("Měření není připravené", string.Join(Environment.NewLine, validation.Issues.Take(5).Select(issue => "• " + issue.Message)), "Rozumím");
                return;
            }

            var path = await _storage.ExportAsync(_package);
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Předat měření do ElektroOffer",
                File = new ShareFile(path)
            });
            SetStatus("Balíček byl vytvořen a je připraven k předání");
        }
        catch (Exception ex)
        {
            SetStatus($"Export se nezdařil: {ex.Message}");
        }
    }

    private void RefreshPage()
    {
        ProjectNameEntry.Text = _package.Project.Name;
        CustomerEntry.Text = _package.Project.CustomerName;
        AddressEntry.Text = _package.Project.SiteAddress;
        TechnicianEntry.Text = _package.Project.TechnicianName;
        ProjectNoteEditor.Text = _package.Project.Note;
        RefreshCatalog();
        RefreshAreas();
    }

    private void RefreshCatalog()
    {
        CatalogPicker.ItemsSource = _catalog?.Options
            .OrderBy(option => option.Kind)
            .ThenBy(option => option.Category)
            .ThenBy(option => option.Name)
            .Select(option => new CatalogChoice(
                option,
                option.Kind == FieldCatalogOptionKind.Work
                    ? $"Práce · úkon · {option.Name}"
                    : $"Materiál · kategorie · {option.Name}"))
            .ToList();
        CatalogStatusLabel.Text = _catalog == null
            ? "Nejprve načtěte katalog z hlavní aplikace"
            : $"Katalog {_catalog.CatalogVersion} · {_catalog.Options.Count} položek";
    }

    private void RefreshAreas()
    {
        AreaPicker.ItemsSource = null;
        AreaPicker.ItemsSource = _package.Project.Areas;
        if (_selectedArea != null)
            AreaPicker.SelectedItem = _package.Project.Areas.FirstOrDefault(area => area.Id == _selectedArea.Id);
        else if (_package.Project.Areas.Count > 0)
            AreaPicker.SelectedIndex = 0;
        else
            RefreshItems();
    }

    private void RefreshItems()
    {
        ItemSection.IsVisible = _selectedArea != null;
        ItemsView.ItemsSource = null;
        ItemsView.ItemsSource = _selectedArea?.Items;
        ItemsView.SelectedItem = _selectedItem;
        RefreshPhotoCount();
    }

    private void RefreshPhotoCount()
    {
        var count = _selectedItem != null
            ? _package.Attachments.Count(photo => photo.ItemId == _selectedItem.Id)
            : _selectedArea == null ? 0 : _package.Attachments.Count(photo => photo.AreaId == _selectedArea.Id);
        PhotoCountLabel.Text = _selectedItem == null ? $"Fotografie místnosti: {count}" : $"Fotografie vybrané položky: {count}";
    }

    private void ClearItemForm()
    {
        ItemNameEntry.Text = string.Empty;
        QuantityEntry.Text = string.Empty;
        UnitEntry.Text = string.Empty;
        ReserveEntry.Text = string.Empty;
        ItemNoteEditor.Text = string.Empty;
        _selectedCatalogOption = null;
        CatalogPicker.SelectedItem = null;
    }

    private void SetStatus(string message) => StatusLabel.Text = message;

    private static string DefaultUnit(MeasurementKind kind) => kind == MeasurementKind.CableRoute ? "m" : "ks";

    private static MeasurementKind InferKind(FieldCatalogOption option)
    {
        var text = option.Name.ToLowerInvariant();
        if (option.Kind is FieldCatalogOptionKind.Material or FieldCatalogOptionKind.MaterialCategory
            && (text.Contains("cyky") || text.Contains("kabel"))) return MeasurementKind.CableRoute;
        if (text.Contains("zásuv")) return MeasurementKind.Socket;
        if (text.Contains("spína") || text.Contains("vypína")) return MeasurementKind.Switch;
        if (text.Contains("svět") || text.Contains("vývod")) return MeasurementKind.Light;
        if (text.Contains("rozvad")) return MeasurementKind.DistributionBoard;
        return MeasurementKind.Custom;
    }

    private sealed record KindOption(MeasurementKind Kind, string Label);
    private sealed record CatalogChoice(FieldCatalogOption Option, string DisplayName);
}
