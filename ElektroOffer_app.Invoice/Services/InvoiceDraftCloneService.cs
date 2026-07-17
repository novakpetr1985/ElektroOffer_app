using System.Collections.ObjectModel;
using ElektroOffer_app.Invoice.Models;

namespace ElektroOffer_app.Invoice.Services
{
    /// <summary>
    /// Vytváří hlubokou kopii faktury pro bezpečné předání mezi oknem, projektem a souborem.
    /// </summary>
    public static class InvoiceDraftCloneService
    {
        public static InvoiceDraft Clone(InvoiceDraft source)
        {
            var clone = new InvoiceDraft
            {
                DraftId = source.DraftId == Guid.Empty ? Guid.NewGuid() : source.DraftId,
                Supplier = CloneParty(source.Supplier),
                Customer = CloneParty(source.Customer),
                Number = source.Number,
                VariableSymbol = source.VariableSymbol,
                IssuedOn = source.IssuedOn,
                TaxableSupplyOn = source.TaxableSupplyOn,
                DueDays = source.DueDays,
                Currency = source.Currency,
                PaymentMethod = source.PaymentMethod,
                Note = source.Note,
                FooterNote = source.FooterNote,
                LogoPath = source.LogoPath,
                OrderNumber = source.OrderNumber,
                ProjectNumber = source.ProjectNumber,
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
                Phone = source.Phone,
                AccountPrefix = source.AccountPrefix,
                AccountNumber = source.AccountNumber,
                BankCode = source.BankCode,
                Iban = source.Iban,
                Swift = source.Swift
            };

        private static InvoiceLine CloneLine(InvoiceLine source)
            => new()
            {
                Type = source.Type,
                Name = source.Name,
                UnitName = source.UnitName,
                Quantity = source.Quantity,
                UnitPrice = source.UnitPrice,
                TotalPriceBeforeDiscount = source.TotalPriceBeforeDiscount,
                TotalPrice = source.TotalPrice,
                VatRate = source.VatRate,
                DiscountPercent = source.DiscountPercent,
                DiscountAmount = source.DiscountAmount
            };
    }
}
