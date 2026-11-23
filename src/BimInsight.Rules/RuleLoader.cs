using System.Text.Json;
using System.Text.Json.Serialization;

namespace BimInsight.Rules;

public static class RuleLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static RuleSet Load(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<RuleSet>(json, Options)
            ?? throw new InvalidDataException("Arquivo de regras inválido.");
    }
}