using ElektroOffer_app.Invoice.Models;

namespace ElektroOffer_app.Invoice.Services;

/// <summary>
/// Looks up public company data without coupling callers to HTTP or the production ARES endpoint.
/// </summary>
public interface IAresClient
{
    Task<InvoiceParty?> FindByRegistrationNoAsync(
        string registrationNo,
        CancellationToken cancellationToken = default);
}
