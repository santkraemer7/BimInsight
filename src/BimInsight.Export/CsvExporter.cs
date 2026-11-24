using System.Globalization;
using System.Text;
using BimInsight.Core.Models;

namespace BimInsight.Export;

public static class CsvExporter
{
    // Separador ";" e números em pt-BR (vírgula decimal) para abrir direto no Excel brasileiro
    public static void Export(
        IEnumerable<BimElement> elements, string path, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.GetCultureInfo("pt-BR");

        var sb = new StringBuilder();
        sb.AppendLine("GlobalId;Tipo IFC;Nome;Tipo;Pavimento;Material;Área (m²);Volume (m³)");

        foreach (var e in elements)
        {
            sb.AppendLine(string.Join(';',
                Escape(e.GlobalId),
                Escape(e.IfcType),
                Escape(e.Name),
                Escape(e.TypeName),
                Escape(e.Storey),
                Escape(e.Material),
                e.Area?.ToString("0.00", culture) ?? "",
                e.Volume?.ToString("0.00", culture) ?? ""));
        }

        // UTF-8 com BOM: o Excel mostra os acentos corretamente
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.IndexOfAny([';', '"', '\n', '\r']) >= 0
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
    }
}