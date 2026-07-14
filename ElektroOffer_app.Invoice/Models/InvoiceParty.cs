using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElektroOffer_app.Invoice.Models
{
    public class InvoiceParty : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _registrationNo = string.Empty;
        private string _vatNo = string.Empty;
        private string _street = string.Empty;
        private string _city = string.Empty;
        private string _zip = string.Empty;
        private string _country = "CZ";
        private string _email = string.Empty;
        private string _phone = string.Empty;

        public string Name { get => _name; set => SetField(ref _name, value); }
        public string RegistrationNo { get => _registrationNo; set => SetField(ref _registrationNo, value); }
        public string VatNo { get => _vatNo; set => SetField(ref _vatNo, value); }
        public string Street { get => _street; set => SetField(ref _street, value); }
        public string City { get => _city; set => SetField(ref _city, value); }
        public string Zip { get => _zip; set => SetField(ref _zip, value); }
        public string Country { get => _country; set => SetField(ref _country, value); }
        public string Email { get => _email; set => SetField(ref _email, value); }
        public string Phone { get => _phone; set => SetField(ref _phone, value); }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetField(ref string field, string value, [CallerMemberName] string? propertyName = null)
        {
            value ??= string.Empty;
            if (field == value) return;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
