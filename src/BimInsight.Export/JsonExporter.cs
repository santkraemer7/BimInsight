using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using BimInsight.Core.Models;
using BimInsight.Quantities;
using BimInsight.Rules;

namespace BimInsight.Export;

public static class JsonExporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // mantém os acentos
        Converters = { new JsonStringEnumConverter() }
    };

    public static void Export(
        IReadOnlyList<BimElement> elements,
        IReadOnlyList<Violation> violations,
        string path)
    {
        var payload = new
        {
            generatedAt = DateTimeOffset.Now,
            elementCount = elements.Count,
            quantitiesByType = QuantityCalculator.Calculate(elements, GroupMode.IfcType),
            quantitiesByStorey = QuantityCalculator.Calculate(elements, GroupMode.Storey),
            quantitiesByMaterial = QuantityCalculator.Calculate(elements, GroupMode.Material),
            elements,
            violations
        };

        File.WriteAllText(path, JsonSerializer.Serialize(payload, Options));
    }
}