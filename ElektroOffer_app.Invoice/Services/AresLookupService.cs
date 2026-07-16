using System.Net;
using System.Net.Http;
using System.Text.Json;
using ElektroOffer_app.Invoice.Models;

namespace ElektroOffer_app.Invoice.Services
{
    public class AresLookupService : IAresClient
    {
        private static readonly HttpClient DefaultClient = new()
        {
            BaseAddress = new Uri("https://ares.gov.cz/ekonomicke-subjekty-v-be/rest/"),
            Timeout = TimeSpan.FromSeconds(15)
        };
        private readonly HttpClient _client;

        public AresLookupService(HttpClient? client = null)
        {
            _client = client ?? DefaultClient;
        }

        public async Task<InvoiceParty?> FindByRegistrationNoAsync(
            string registrationNo,
            CancellationToken cancellationToken = default)
        {
            var ico = NormalizeRegistrationNo(registrationNo);
            if (ico == null)
                return null;

            using var response = await _client.GetAsync(
                $"ekonomicke-subjekty/{ico}",
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = document.RootElement;

            var party = new InvoiceParty
            {
                RegistrationNo = GetString(root, "ico") ?? ico,
                Name = GetString(root, "obchodniJmeno") ?? string.Empty,
                VatNo = GetString(root, "dic") ?? string.Empty,
                Country = "CZ"
            };

            if (root.TryGetProperty("sidlo", out var address))
            {
                party.Street = BuildStreet(address);
                party.City = BuildCity(address);
                party.Zip = GetString(address, "psc") ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(party.Street) &&
                root.TryGetProperty("adresaDorucovaci", out var deliveryAddress))
            {
                party.Street = GetString(deliveryAddress, "radekAdresy1") ?? string.Empty;
                party.City = GetString(deliveryAddress, "radekAdresy3") ?? string.Empty;
            }

            return party;
        }

        public static string? NormalizeRegistrationNo(string? value)
        {
            var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
            return digits.Length == 8 && IsValidRegistrationNo(digits)
                ? digits
                : null;
        }

        private static bool IsValidRegistrationNo(string ico)
        {
            var sum = 0;
            for (var i = 0; i < 7; i++)
                sum += (ico[i] - '0') * (8 - i);

            var check = 11 - (sum % 11);
            var expected = check switch
            {
                10 => 0,
                11 => 1,
                _ => check
            };

            return expected == ico[7] - '0';
        }

        private static string BuildStreet(JsonElement address)
        {
            var street = GetString(address, "nazevUlice") ?? GetString(address, "nazevCastiObce") ?? string.Empty;
            var houseNumber = GetString(address, "cisloDomovni");
            var orientation = GetString(address, "cisloOrientacni");
            var orientationLetter = GetString(address, "cisloOrientacniPismeno");

            var number = houseNumber ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(orientation))
                number += "/" + orientation + orientationLetter;

            return string.Join(" ", new[] { street, number }.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static string BuildCity(JsonElement address)
        {
            var city = GetString(address, "nazevObce") ?? string.Empty;
            var district = GetString(address, "nazevMestskehoObvodu");

            if (!string.IsNullOrWhiteSpace(district) && !string.Equals(city, district, StringComparison.OrdinalIgnoreCase))
                return $"{city} {district}".Trim();

            return city;
        }

        private static string? GetString(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
                return null;

            return property.ValueKind switch
            {
                JsonValueKind.String => property.GetString(),
                JsonValueKind.Number => property.GetRawText(),
                _ => null
            };
        }
    }
}
