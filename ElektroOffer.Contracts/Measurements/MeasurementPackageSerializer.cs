using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElektroOffer.Contracts.Measurements;

public static class MeasurementPackageSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static string Serialize(MeasurementPackage package) =>
        JsonSerializer.Serialize(package, Options);

    public static MeasurementPackage Deserialize(string json) =>
        JsonSerializer.Deserialize<MeasurementPackage>(json, Options)
        ?? throw new InvalidDataException("Measurement package is empty or invalid.");
}
