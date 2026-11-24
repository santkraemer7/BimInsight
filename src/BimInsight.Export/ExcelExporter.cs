using BimInsight.Core.Models;
using BimInsight.Quantities;
using BimInsight.Rules;
using ClosedXML.Excel;

namespace BimInsight.Export;

public static class ExcelExporter
{
    public static void Export(
        IReadOnlyList<BimElement> elements,
        IReadOnlyList<Violation> violations,
        string path)
    {
        using var wb = new XLWorkbook();

        WriteElements(wb.Worksheets.Add("Elementos"), elements);
        WriteQuantities(wb.Worksheets.Add("Por tipo IFC"), elements, GroupMode.IfcType);
        WriteQuantities(wb.Worksheets.Add("Por pavimento"), elements, GroupMode.Storey);
        WriteQuantities(wb.Worksheets.Add("Por material"), elements, GroupMode.Material);

        if (violations.Count > 0)
            WriteViolations(wb.Worksheets.Add("Violações"), violations);

        wb.SaveAs(path);
    }

    private static void WriteHeader(IXLWorksheet ws, params string[] headers)
    {
        for (var i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).Value = headers[i];

        var header = ws.Range(1, 1, 1, headers.Length);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
        ws.SheetView.FreezeRows(1);
    }

    private static void Finish(IXLWorksheet ws, bool filter = true)
    {
        ws.Columns().AdjustToContents();
        if (filter) ws.RangeUsed()?.SetAutoFilter();
    }

    private static void WriteElements(IXLWorksheet ws, IReadOnlyList<BimElement> elements)
    {
        WriteHeader(ws, "GlobalId", "Tipo IFC", "Nome", "Tipo", "Pavimento",
                        "Material", "Área (m²)", "Volume (m³)");

        var row = 2;
        foreach (var e in elements)
        {
            ws.Cell(row, 1).Value = e.GlobalId;
            ws.Cell(row, 2).Value = e.IfcType;
            ws.Cell(row, 3).Value = e.Name ?? "";
            ws.Cell(row, 4).Value = e.TypeName ?? "";
            ws.Cell(row, 5).Value = e.Storey ?? "";
            ws.Cell(row, 6).Value = e.Material ?? "";
            if (e.Area is double a) ws.Cell(row, 7).Value = a;
            if (e.Volume is double v) ws.Cell(row, 8).Value = v;
            row++;
        }

        ws.Columns("G:H").Style.NumberFormat.Format = "#,##0.00";
        Finish(ws);
    }

    private static void WriteQuantities(
        IXLWorksheet ws, IReadOnlyList<BimElement> elements, GroupMode mode)
    {
        var rows = QuantityCalculator.Calculate(elements, mode);

        WriteHeader(ws, "Grupo", "Qtd", "Área (m²)", "Volume (m³)");

        var row = 2;
        foreach (var r in rows)
        {
            ws.Cell(row, 1).Value = r.Group;
            ws.Cell(row, 2).Value = r.Count;
            ws.Cell(row, 3).Value = r.TotalArea;
            ws.Cell(row, 4).Value = r.TotalVolume;
            row++;
        }

        ws.Cell(row, 1).Value = "Total";
        ws.Cell(row, 2).Value = rows.Sum(r => r.Count);
        ws.Cell(row, 3).Value = rows.Sum(r => r.TotalArea);
        ws.Cell(row, 4).Value = rows.Sum(r => r.TotalVolume);
        ws.Range(row, 1, row, 4).Style.Font.Bold = true;

        ws.Columns("C:D").Style.NumberFormat.Format = "#,##0.00";
        Finish(ws, filter: false);
    }

    private static void WriteViolations(IXLWorksheet ws, IReadOnlyList<Violation> violations)
    {
        WriteHeader(ws, "Severidade", "Regra", "Descrição", "Tipo IFC", "Elemento", "GlobalId");

        var row = 2;
        foreach (var v in violations.OrderByDescending(v => v.Severity))
        {
            ws.Cell(row, 1).Value = v.Severity.ToString();
            ws.Cell(row, 2).Value = v.RuleId;
            ws.Cell(row, 3).Value = v.Description;
            ws.Cell(row, 4).Value = v.IfcType;
            ws.Cell(row, 5).Value = v.ElementName ?? "";
            ws.Cell(row, 6).Value = v.GlobalId;
            row++;
        }

        Finish(ws);
    }
}