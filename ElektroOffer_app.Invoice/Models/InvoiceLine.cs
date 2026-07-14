using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElektroOffer_app.Invoice.Models
{
    public class InvoiceLine : INotifyPropertyChanged
    {
        private string _type = string.Empty;
        private string _name = string.Empty;
        private string _unitName = string.Empty;
        private double _quantity;
        private decimal _unitPrice;
        private decimal _totalPrice;
        private decimal _vatRate;
        private double? _discountPercent;
        private decimal? _discountAmount;

        public string Type { get => _type; set => SetField(ref _type, value); }
        public string Name { get => _name; set => SetField(ref _name, value); }
        public string UnitName { get => _unitName; set => SetField(ref _unitName, value); }
        public double Quantity { get => _quantity; set => SetField(ref _quantity, value); }
        public decimal UnitPrice { get => _unitPrice; set => SetField(ref _unitPrice, value); }
        public decimal TotalPrice { get => _totalPrice; set => SetField(ref _totalPrice, value); }
        public decimal VatRate { get => _vatRate; set => SetField(ref _vatRate, value); }
        public double? DiscountPercent { get => _discountPercent; set => SetField(ref _discountPercent, value); }
        public decimal? DiscountAmount { get => _discountAmount; set => SetField(ref _discountAmount, value); }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
