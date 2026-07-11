using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ElektroOffer_app.Commands;
using ElektroOffer_app.Models;
using ElektroOffer_app.Services;
using ElektroOffer_app.ViewModels.Items;
using Microsoft.Win32; // SaveFileDialog

namespace ElektroOffer_app.ViewModels
{
    // ============================================================================
    // 🧾 InvoiceWindowViewModel – formulář pro vygenerování faktury
    // ----------------------------------------------------------------------------
    // Účel:
    //   • Sbírá fakturační údaje (InvoiceItemData), údaje dodavatele
    //     (SupplierInfo) a řádky (InvoiceLineData), spouští generování PDF
    //     přes InvoiceTemplateService.
    //   • Vše se edituje přímo v jednom okně (InvoiceWindow) – samostatné
    //     SupplierSettingsWindow bylo zrušeno jako zbytečná duplicita.
    //
    // 🔴 SupplierInfo (dříve pojmenováno "Supplier"):
    //   • Přejmenováno kvůli konfliktu s existující EF entitou Supplier.cs
    //     (dodavatel materiálu v cenové kaskádě) – CS0119 "Supplier je typ".
    //   • Načítá se přes SupplierSettingsService.Load() při otevření okna,
    //     a ukládá se zpět přes .Save() při každém generování PDF, aby se
    //     údaje firmy pamatovaly i pro příští fakturu (uživatel je nemusí
    //     přepisovat pokaždé znovu).
    //
    // 🔴 VALIDACE – přístup "upozornit, ne blokovat":
    //   • Tlačítko "Vygenerovat PDF" je VŽDY aktivní.
    //   • Při kliknutí GetValidationWarnings() zkontroluje běžně očekávaná
    //     pole a pokud něco chybí, zobrazí se seznam s dotazem
    //     "Pokračovat i přesto?". Uživatel má poslední slovo.
    // ============================================================================
    public class InvoiceWindowViewModel : INotifyPropertyChanged
    {
        private readonly InvoiceTemplateService _templateService = new();
        private readonly SupplierSettingsService _supplierService = new();

        public InvoiceWindowViewModel(
            IEnumerable<CalculationItemViewModel> workItems,
            IEnumerable<CalculationItemViewModel> materialItems)
        {
            // Předvyplnění údajů dodavatele z posledně uložených hodnot
            SupplierInfo = _supplierService.Load();

            var initialLines = _templateService.BuildInvoiceLines(workItems, materialItems);
            Lines = new ObservableCollection<InvoiceLineData>(initialLines);

            GeneratePdfCommand = new RelayCommand(_ => TryGeneratePdf());
            AddManualLineCommand = new RelayCommand(_ => AddManualLine());
            RemoveLineCommand = new RelayCommand(param => RemoveLine(param as InvoiceLineData));
        }

        // =========================================================
        // 🏢 DODAVATEL – editovatelný přímo v tomto okně
        // =========================================================
        //
        // Přejmenováno z "Supplier" na "SupplierInfo" kvůli konfliktu
        // s EF entitou Supplier.cs (viz komentář u třídy). V XAML musí
        // binding odpovídajícím způsobem používat "SupplierInfo.*",
        // ne "Supplier.*".
        //
        public SupplierSettings SupplierInfo { get; set; }

        // =========================================================
        // FAKTURAČNÍ ÚDAJE (InvoiceItemData pole jednotlivě)
        // =========================================================

        private string _invoiceNumber = string.Empty;
        public string InvoiceNumber
        {
            get => _invoiceNumber;
            set { _invoiceNumber = value; OnPropertyChanged(); }
        }

        private DateTime _issueDate = DateTime.Now;
        public DateTime IssueDate
        {
            get => _issueDate;
            set { _issueDate = value; OnPropertyChanged(); }
        }

        private DateTime _dueDate = DateTime.Now.AddDays(14);
        public DateTime DueDate
        {
            get => _dueDate;
            set { _dueDate = value; OnPropertyChanged(); }
        }

        private string _customerName = string.Empty;
        public string CustomerName
        {
            get => _customerName;
            set { _customerName = value; OnPropertyChanged(); }
        }

        private string _customerAddress = string.Empty;
        public string CustomerAddress
        {
            get => _customerAddress;
            set { _customerAddress = value; OnPropertyChanged(); }
        }

        private string? _customerIco;
        public string? CustomerIco
        {
            get => _customerIco;
            set { _customerIco = value; OnPropertyChanged(); }
        }

        private string? _customerDic;
        public string? CustomerDic
        {
            get => _customerDic;
            set { _customerDic = value; OnPropertyChanged(); }
        }

        private string? _note;
        public string? Note
        {
            get => _note;
            set { _note = value; OnPropertyChanged(); }
        }

        // =========================================================
        // ŘÁDKY FAKTURY
        // =========================================================

        public ObservableCollection<InvoiceLineData> Lines { get; }

        // =========================================================
        // COMMANDS
        // =========================================================

        public ICommand GeneratePdfCommand { get; }
        public ICommand AddManualLineCommand { get; }
        public ICommand RemoveLineCommand { get; }

        // ------------------------------------------------------------------
        // ⚠ GetValidationWarnings – co vypadá neúplně (nic neblokuje samo)
        // ------------------------------------------------------------------
        private List<string> GetValidationWarnings()
        {
            var warnings = new List<string>();

            if (string.IsNullOrWhiteSpace(InvoiceNumber))
                warnings.Add("Chybí číslo faktury.");

            if (string.IsNullOrWhiteSpace(CustomerName))
                warnings.Add("Chybí jméno/název odběratele.");

            if (string.IsNullOrWhiteSpace(SupplierInfo.CompanyName))
                warnings.Add("Chybí název firmy dodavatele.");

            if (Lines.Count == 0)
                warnings.Add("Faktura neobsahuje žádné řádky.");

            for (int i = 0; i < Lines.Count; i++)
            {
                var line = Lines[i];
                var rowLabel = $"Řádek {i + 1}";

                if (string.IsNullOrWhiteSpace(line.Description))
                    warnings.Add($"{rowLabel}: chybí popis.");

                if (line.UnitPrice <= 0)
                    warnings.Add($"{rowLabel}: cena je nulová.");

                if (line.Quantity <= 0)
                    warnings.Add($"{rowLabel}: množství je nulové – zkontroluj, zda je to záměr.");
            }

            return warnings;
        }

        // ------------------------------------------------------------------
        // 🖨 TryGeneratePdf – zkontroluje, případně se zeptá, pak generuje
        // ------------------------------------------------------------------
        private void TryGeneratePdf()
        {
            var warnings = GetValidationWarnings();

            if (warnings.Count > 0)
            {
                var message = "Faktura vypadá neúplně:\n\n"
                    + string.Join("\n", warnings.Select(w => $"• {w}"))
                    + "\n\nPokračovat i přesto?";

                var result = MessageBox.Show(
                    message,
                    "Neúplné údaje faktury",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            GeneratePdf();
        }

        // ------------------------------------------------------------------
        // ➕ AddManualLine / RemoveLine
        // ------------------------------------------------------------------
        private void AddManualLine()
        {
            Lines.Add(new InvoiceLineData
            {
                LineType = "Jiné",
                Description = string.Empty,
                Quantity = 0,
                Unit = string.Empty,
                UnitPrice = 0m
            });
        }

        private void RemoveLine(InvoiceLineData? line)
        {
            if (line == null) return;
            Lines.Remove(line);
        }

        // ------------------------------------------------------------------
        // 🖨 GeneratePdf – uloží dodavatele, sestaví InvoiceItemData,
        //    spustí generování PDF
        // ------------------------------------------------------------------
        private void GeneratePdf()
        {
            // 🔴 Uloží aktuální (případně upravené) údaje dodavatele,
            //    aby se pamatovaly i pro příští fakturu.
            _supplierService.Save(SupplierInfo);

            var invoiceData = new InvoiceItemData
            {
                InvoiceNumber = InvoiceNumber,
                IssueDate = IssueDate,
                DueDate = DueDate,
                CustomerName = CustomerName,
                CustomerAddress = CustomerAddress,
                CustomerIco = CustomerIco,
                CustomerDic = CustomerDic,
                Note = Note,
                Lines = Lines.ToList()
            };

            var dialog = new SaveFileDialog
            {
                Filter = "PDF soubor (*.pdf)|*.pdf",
                FileName = $"Faktura_{InvoiceNumber}.pdf"
            };

            if (dialog.ShowDialog() != true)
                return;

            _templateService.GeneratePdf(invoiceData, SupplierInfo, dialog.FileName);
        }

        // =========================================================
        // NOTIFY
        // =========================================================

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}