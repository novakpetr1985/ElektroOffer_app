using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElektroOffer_app.Invoice.Models
{
    public class InvoiceDraft : INotifyPropertyChanged
    {
        private InvoiceParty _supplier = new();
        private InvoiceParty _customer = new();
        private string _number = string.Empty;
        private string _variableSymbol = string.Empty;
        private DateTime _issuedOn = DateTime.Today;
        private int _dueDays = 14;
        private string _currency = "CZK";
        private string _paymentMethod = "bank";
        private string _note = string.Empty;
        private string _footerNote = string.Empty;

        public InvoiceParty Supplier { get => _supplier; set => SetField(ref _supplier, value); }
        public InvoiceParty Customer { get => _customer; set => SetField(ref _customer, value); }
        public string Number { get => _number; set => SetField(ref _number, value); }
        public string VariableSymbol { get => _variableSymbol; set => SetField(ref _variableSymbol, value); }
        public DateTime IssuedOn { get => _issuedOn; set => SetField(ref _issuedOn, value); }
        public int DueDays { get => _dueDays; set => SetField(ref _dueDays, value); }
        public string Currency { get => _currency; set => SetField(ref _currency, value); }
        public string PaymentMethod { get => _paymentMethod; set => SetField(ref _paymentMethod, value); }
        public string Note { get => _note; set => SetField(ref _note, value); }
        public string FooterNote { get => _footerNote; set => SetField(ref _footerNote, value); }
        public ObservableCollection<InvoiceLine> Lines { get; set; } = new();

        public decimal Total => Lines.Sum(line => line.TotalPrice);

        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
