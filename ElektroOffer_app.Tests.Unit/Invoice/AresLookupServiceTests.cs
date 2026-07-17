using System.Net;
using System.Text;
using ElektroOffer_app.Invoice.Services;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.Invoice;

[TestFixture]
/// <summary>Ověřuje mapování a chybové stavy ARES klienta bez síťových požadavků.</summary>
public class AresLookupServiceTests
{
    [Test]
    public async Task FindByRegistrationNoAsync_Should_Map_Official_Rest_Response()
    {
        var service = CreateService(HttpStatusCode.OK, Fixture("ares-valid-company.json"));

        var party = await service.FindByRegistrationNoAsync("27074358");

        Assert.Multiple(() =>
        {
            Assert.That(party, Is.Not.Null);
            Assert.That(party!.Name, Is.EqualTo("Test Company s.r.o."));
            Assert.That(party.VatNo, Is.EqualTo("CZ27074358"));
            Assert.That(party.Street, Is.EqualTo("Testovací 10/2"));
            Assert.That(party.City, Is.EqualTo("Praha"));
            Assert.That(party.Zip, Is.EqualTo("11000"));
        });
    }

    [Test]
    public async Task FindByRegistrationNoAsync_Should_Return_Null_For_NotFound()
    {
        var service = CreateService(HttpStatusCode.NotFound, Fixture("ares-not-found.json"));

        var party = await service.FindByRegistrationNoAsync("27074358");

        Assert.That(party, Is.Null);
    }

    [Test]
    public async Task FindByRegistrationNoAsync_Should_Map_NonVatPayer_And_Numeric_Zip()
    {
        var service = CreateService(HttpStatusCode.OK, Fixture("ares-non-vat-payer.json"));

        var party = await service.FindByRegistrationNoAsync("25596641");

        Assert.Multiple(() =>
        {
            Assert.That(party, Is.Not.Null);
            Assert.That(party!.VatNo, Is.Empty);
            Assert.That(party.City, Is.EqualTo("Brno"));
            Assert.That(party.Zip, Is.EqualTo("60200"));
        });
    }

    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.TooManyRequests)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public void FindByRegistrationNoAsync_Should_Expose_Http_Failure(HttpStatusCode statusCode)
    {
        var service = CreateService(statusCode, "server error");

        Assert.ThrowsAsync<HttpRequestException>(async () =>
            await service.FindByRegistrationNoAsync("27074358"));
    }

    [Test]
    public void FindByRegistrationNoAsync_Should_Reject_Invalid_Json()
    {
        var service = CreateService(HttpStatusCode.OK, Fixture("ares-invalid-response.json"));

        Assert.CatchAsync<System.Text.Json.JsonException>(async () =>
            await service.FindByRegistrationNoAsync("27074358"));
    }

    [Test]
    public async Task FindByRegistrationNoAsync_Should_Handle_Incomplete_Response()
    {
        var service = CreateService(HttpStatusCode.OK, Fixture("ares-incomplete-response.json"));

        var party = await service.FindByRegistrationNoAsync("27074358");

        Assert.Multiple(() =>
        {
            Assert.That(party!.Name, Is.EqualTo("Incomplete Company"));
            Assert.That(party.Street, Is.Empty);
            Assert.That(party.City, Is.Empty);
            Assert.That(party.VatNo, Is.Empty);
        });
    }

    [Test]
    public void FindByRegistrationNoAsync_Should_Respect_Cancellation()
    {
        var handler = new StubHttpMessageHandler(async (_, cancellationToken) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://ares.gov.cz/ekonomicke-subjekty-v-be/rest/")
        };
        var service = new AresLookupService(client);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await service.FindByRegistrationNoAsync("27074358", cancellation.Token));
    }

    [Test]
    public void FindByRegistrationNoAsync_Should_Respect_HttpClient_Timeout()
    {
        var handler = new StubHttpMessageHandler(async (_, cancellationToken) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://ares.gov.cz/ekonomicke-subjekty-v-be/rest/"),
            Timeout = TimeSpan.FromMilliseconds(50)
        };
        var service = new AresLookupService(client);

        Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await service.FindByRegistrationNoAsync("27074358"));
    }

    private static AresLookupService CreateService(HttpStatusCode statusCode, string content)
    {
        var handler = new StubHttpMessageHandler((_, _) => Task.FromResult(
            new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            }));
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://ares.gov.cz/ekonomicke-subjekty-v-be/rest/")
        };
        return new AresLookupService(client);
    }

    private static string Fixture(string name)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", "Ares", name));

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => responseFactory(request, cancellationToken);
    }
}
