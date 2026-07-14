using System.Collections.ObjectModel;
using ElektroOffer_app.Invoice.Models;

namespace ElektroOffer_app.Invoice.Services
{
    public static class InvoiceDraftCloneService
    {
        public static InvoiceDraft Clone(InvoiceDraft source)
        {
            var clone = new InvoiceDraft
            {
                Supplier = CloneParty(source.Supplier),
                Customer = CloneParty(source.Customer),
                Number = source.Number,
                VariableSymbol = source.VariableSymbol,
                IssuedOn = source.IssuedOn,
                DueDays = source.DueDays,
                Currency = source.Currency,
                PaymentMethod = source.PaymentMethod,
                Note = source.Note,
                FooterNote = source.FooterNote,
                Lines = new ObservableCollection<InvoiceLine>(source.Lines.Select(CloneLine))
            };

            return clone;
        }

        private static InvoiceParty CloneParty(InvoiceParty source)
            => new()
            {
                Name = source.Name,
                RegistrationNo = source.RegistrationNo,
                VatNo = source.VatNo,
                Street = source.Street,
                City = source.City,
                Zip = source.Zip,
                Country = source.Country,
                Email = source.Email,
                Phone = source.Phone
            };

        private static InvoiceLine CloneLine(InvoiceLine source)
            => new()
            {
                Type = source.Type,
                Name = source.Name,
                UnitName = source.UnitName,
                Quantity = source.Quantity,
                UnitPrice = source.UnitPrice,
                TotalPrice = source.TotalPrice,
                VatRate = source.VatRate,
                DiscountPercent = source.DiscountPercent,
                DiscountAmount = source.DiscountAmount
            };
    }
}
