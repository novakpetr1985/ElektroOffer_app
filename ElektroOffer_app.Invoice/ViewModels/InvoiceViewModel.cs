using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ElektroOffer_app.Invoice.Models;
using ElektroOffer_app.Invoice.Services;
using Microsoft.Win32;

namespace ElektroOffer_app.Invoice.ViewModels
{
    public class InvoiceViewModel : INotifyPropertyChanged
    {
        private readonly FakturoidExportService _fakturoidExport = new();
        private readonly IAresClient _aresLookup;
        private readonly InvoiceFileService _invoiceFiles = new();
        private readonly PdfInvoiceExportService _pdfExport = new();
        private string _statusText = "Navrh faktury pripraven";
        private bool _isInitializing = true;
        private bool _hasUnsavedChanges;
        private string? _currentInvoicePath;

        public InvoiceViewModel(
            IEnumerable<InvoiceSourceItem> sourceItems,
            InvoiceDraft? savedDraft = null,
            IAresClient? aresClient = null)
        {
            _aresLookup = aresClient ?? new AresLookupService();
            Draft = savedDraft != null
                ? InvoiceDraftCloneService.Clone(savedDraft)
                : CreateDraftFromSource(sourceItems);

            AttachChangeTracking();
            ExportFakturoidJsonCommand = new RelayCommand(_ => ExportFakturoidJson());
            ExportPdfCommand = new RelayCommand(_ => ExportPdf());
            SaveInvoiceCommand = new RelayCommand(_ => SaveInvoice());
            SaveInvoiceAsCommand = new RelayCommand(_ => SaveInvoiceAs());
            LoadInvoiceCommand = new RelayCommand(_ => LoadInvoice());
            SaveToProjectCommand = new RelayCommand(_ => SaveToProject());
            LookupSupplierAresCommand = new RelayCommand(async _ => await LookupAresAsync(Supplier));
            LookupCustomerAresCommand = new RelayCommand(async _ => await LookupAresAsync(Customer));
            _isInitializing = false;
        }

        public InvoiceDraft Draft { get; private set; }
        public InvoiceParty Supplier => Draft.Supplier;
        public InvoiceParty Customer => Draft.Customer;
        public ObservableCollection<InvoiceLine> Lines => Draft.Lines;
        public ICommand ExportFakturoidJsonCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand SaveInvoiceCommand { get; }
        public ICommand SaveInvoiceAsCommand { get; }
        public ICommand LoadInvoiceCommand { get; }
        public ICommand SaveToProjectCommand { get; }
        public ICommand LookupSupplierAresCommand { get; }
        public ICommand LookupCustomerAresCommand { get; }

        public decimal Total => Draft.Total;
        public event EventHandler<InvoiceDraft>? SaveToProjectRequested;

        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            private set
            {
                if (_hasUnsavedChanges == value) return;
                _hasUnsavedChanges = value;
                OnPropertyChanged();
            }
        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                if (_statusText == value) return;
                _statusText = value;
                OnPropertyChanged();
            }
        }

        private void ExportFakturoidJson()
        {
            var dialog = new SaveFileDialog
            {
                Title = "Ulozit Fakturoid JSON",
                Filter = "Fakturoid JSON (*.json)|*.json|Vsechny soubory (*.*)|*.*",
                DefaultExt = ".json",
                FileName = string.IsNullOrWhiteSpace(Draft.Number)
                    ? $"fakturoid_invoice_{DateTime.Now:yyyy-MM-dd}.json"
                    : $"{Draft.Number}.json"
            };

            if (dialog.ShowDialog() != true)
                return;

            File.WriteAllText(dialog.FileName, _fakturoidExport.BuildJson(Draft));
            StatusText = $"Ulozeno: {dialog.FileName}";
        }

        private void ExportPdf()
        {
            var dialog = new SaveFileDialog
            {
                Title = "Ulozit fakturu do PDF",
                Filter = "PDF (*.pdf)|*.pdf|Vsechny soubory (*.*)|*.*",
                DefaultExt = ".pdf",
                FileName = string.IsNullOrWhiteSpace(Draft.Number)
                    ? $"faktura_{DateTime.Now:yyyy-MM-dd}.pdf"
                    : $"{Draft.Number}.pdf"
            };

            if (dialog.ShowDialog() != true)
                return;

            _pdfExport.Export(dialog.FileName, Draft);
            StatusText = $"PDF ulozeno: {dialog.FileName}";
        }

        private void SaveInvoice()
        {
            if (string.IsNullOrWhiteSpace(_currentInvoicePath))
            {
                SaveInvoiceAs();
                return;
            }

            SaveInvoiceToPath(_currentInvoicePath);
        }

        private void SaveInvoiceAs()
        {
            var dialog = new SaveFileDialog
            {
                Title = "Ulozit fakturacni data",
                Filter = "Fakturace ElektroOffer (*.eofinvoice)|*.eofinvoice|Vsechny soubory (*.*)|*.*",
                DefaultExt = ".eofinvoice",
                FileName = string.IsNullOrWhiteSpace(Draft.Number)
                    ? $"fakturace_{DateTime.Now:yyyy-MM-dd}.eofinvoice"
                    : $"{Draft.Number}.eofinvoice"
            };

            if (dialog.ShowDialog() != true)
                return;

            SaveInvoiceToPath(dialog.FileName);
        }

        private void SaveInvoiceToPath(string path)
        {
            _invoiceFiles.Save(path, Draft);
            _currentInvoicePath = path;
            HasUnsavedChanges = false;
            StatusText = $"Fakturacni data ulozena: {path}";
        }

        private void LoadInvoice()
        {
            if (!ConfirmDiscardChanges())
                return;

            var dialog = new OpenFileDialog
            {
                Title = "Nacist fakturacni data",
                Filter = "Fakturace ElektroOffer (*.eofinvoice)|*.eofinvoice|Vsechny soubory (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true)
                return;

            var loaded = _invoiceFiles.Load(dialog.FileName);
            if (loaded == null)
            {
                StatusText = "Fakturacni data se nepodarilo nacist";
                return;
            }

            ReplaceDraft(loaded);
            _currentInvoicePath = dialog.FileName;
            HasUnsavedChanges = false;
            StatusText = $"Nacteno: {dialog.FileName}";
        }

        private void SaveToProject()
        {
            HasUnsavedChanges = false;
            SaveToProjectRequested?.Invoke(this, InvoiceDraftCloneService.Clone(Draft));
            StatusText = "Fakturacni data ulozena do projektu";
        }

        private async Task LookupAresAsync(InvoiceParty party)
        {
            if (AresLookupService.NormalizeRegistrationNo(party.RegistrationNo) == null)
            {
                StatusText = "Zadejte platne ICO ve formatu 8 cislic";
                return;
            }

            try
            {
                StatusText = "Vyhledavam v ARES...";
                var found = await _aresLookup.FindByRegistrationNoAsync(party.RegistrationNo);

                if (found == null)
                {
                    StatusText = "ARES nenasel zaznam pro zadane ICO";
                    return;
                }

                party.Name = found.Name;
                party.RegistrationNo = found.RegistrationNo;
                party.VatNo = found.VatNo;
                party.Street = found.Street;
                party.City = found.City;
                party.Zip = found.Zip;
                party.Country = found.Country;
                StatusText = "Udaje doplneny z ARES";
            }
            catch (Exception ex)
            {
                StatusText = $"ARES chyba: {ex.Message}";
            }
        }

        public bool ConfirmDiscardChanges()
        {
            if (!HasUnsavedChanges)
                return true;

            var result = MessageBox.Show(
                "Fakturace obsahuje neulozene zmeny. Ulozit je do samostatneho souboru?",
                "Neulozena fakturace",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Cancel)
                return false;

            if (result == MessageBoxResult.Yes)
            {
                SaveInvoice();
                return !HasUnsavedChanges;
            }

            return true;
        }

        private static InvoiceDraft CreateDraftFromSource(IEnumerable<InvoiceSourceItem> sourceItems)
        {
            var draft = new InvoiceDraft
            {
                Note = "Fakturujeme Vam polozky dle detailniho rozpoctu.",
                FooterNote = "Vystaveno z ElektroOffer."
            };

            foreach (var source in sourceItems.Where(item => item.Price > 0))
            {
                var quantity = source.Quantity <= 0 ? 1 : source.Quantity;
                var total = Convert.ToDecimal(source.Price);
                var totalBeforeDiscount = Convert.ToDecimal(source.PriceBeforeDiscount > 0
                    ? source.PriceBeforeDiscount
                    : source.Price);

                draft.Lines.Add(new InvoiceLine
                {
                    Type = source.Type,
                    Name = string.IsNullOrWhiteSpace(source.Description)
                        ? source.Type
                        : $"{source.Type}: {source.Description}",
                    UnitName = source.Unit,
                    Quantity = quantity,
                    UnitPrice = Math.Round(totalBeforeDiscount / Convert.ToDecimal(quantity), 2),
                    TotalPriceBeforeDiscount = totalBeforeDiscount,
                    TotalPrice = total,
                    VatRate = 0,
                    DiscountPercent = source.DiscountPercent,
                    DiscountAmount = source.DiscountAmount.HasValue
                        ? Convert.ToDecimal(source.DiscountAmount.Value)
                        : null
                });
            }

            return draft;
        }

        private void ReplaceDraft(InvoiceDraft draft)
        {
            DetachChangeTracking();
            Draft = draft;
            AttachChangeTracking();
            OnPropertyChanged(nameof(Draft));
            OnPropertyChanged(nameof(Supplier));
            OnPropertyChanged(nameof(Customer));
            OnPropertyChanged(nameof(Lines));
            OnPropertyChanged(nameof(Total));
        }

        private void AttachChangeTracking()
        {
            Draft.PropertyChanged += Draft_PropertyChanged;
            Draft.Supplier.PropertyChanged += Draft_PropertyChanged;
            Draft.Customer.PropertyChanged += Draft_PropertyChanged;
            Draft.Lines.CollectionChanged += Lines_CollectionChanged;

            foreach (var line in Draft.Lines)
                line.PropertyChanged += Draft_PropertyChanged;
        }

        private void DetachChangeTracking()
        {
            Draft.PropertyChanged -= Draft_PropertyChanged;
            Draft.Supplier.PropertyChanged -= Draft_PropertyChanged;
            Draft.Customer.PropertyChanged -= Draft_PropertyChanged;
            Draft.Lines.CollectionChanged -= Lines_CollectionChanged;

            foreach (var line in Draft.Lines)
                line.PropertyChanged -= Draft_PropertyChanged;
        }

        private void Lines_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (InvoiceLine line in e.NewItems)
                    line.PropertyChanged += Draft_PropertyChanged;
            }

            if (e.OldItems != null)
            {
                foreach (InvoiceLine line in e.OldItems)
                    line.PropertyChanged -= Draft_PropertyChanged;
            }

            MarkAsChanged();
            OnPropertyChanged(nameof(Total));
        }

        private void Draft_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            MarkAsChanged();
            OnPropertyChanged(nameof(Total));
        }

        private void MarkAsChanged()
        {
            if (_isInitializing)
                return;

            HasUnsavedChanges = true;
            if (!StatusText.EndsWith(" *", StringComparison.Ordinal))
                StatusText += " *";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private sealed class RelayCommand : ICommand
        {
            private readonly Action<object?> _execute;

            public RelayCommand(Action<object?> execute)
            {
                _execute = execute;
            }

            public bool CanExecute(object? parameter) => true;
            public void Execute(object? parameter) => _execute(parameter);
            public event EventHandler? CanExecuteChanged
            {
                add { }
                remove { }
            }
        }
    }
}
