using BimInsight.Core.Models;
using BimInsight.Rules;

namespace BimInsight.Export;

public static class ExportService
{
    public static void Export(
        string path,
        IReadOnlyList<BimElement> elements,
        IReadOnlyList<Violation> violations)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();

        if (extension is not (".xlsx" or ".csv" or ".json"))
            throw new NotSupportedException(
                $"Formato não suportado: '{extension}'. Use .xlsx, .csv ou .json.");

        var folder = Path.GetDirectoryName(Path.GetFullPath(path))!;
        Directory.CreateDirectory(folder);

        switch (extension)
        {
            case ".xlsx": ExcelExporter.Export(elements, violations, path); break;
            case ".csv":  CsvExporter.Export(elements, path); break;
            case ".json": JsonExporter.Export(elements, violations, path); break;
        }
    }
}