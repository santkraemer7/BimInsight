using System.Globalization;
using BimInsight.Core.Models;
using BimInsight.Export;
using BimInsight.Rules;
using ClosedXML.Excel;
using Xunit;

namespace BimInsight.Tests;

public class ExporterTests
{
    private static readonly BimElement[] Sample =
    {
        new("1", "IFCWALL", "Parede A", "Parede 20", "Térreo", "Concreto", 10.5, 2)
    };

    [Fact]
    public void Csv_gera_cabecalho_e_uma_linha_por_elemento()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv");
        try
        {
            CsvExporter.Export(Sample, path, CultureInfo.InvariantCulture);

            var lines = File.ReadAllLines(path);
            Assert.Equal(2, lines.Length);
            Assert.Contains("10.50", lines[1]);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Excel_grava_os_elementos_na_aba_Elementos()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");
        try
        {
            ExcelExporter.Export(Sample, Array.Empty<Violation>(), path);

            using var wb = new XLWorkbook(path);
            var ws = wb.Worksheet("Elementos");
            Assert.Equal("1", ws.Cell(2, 1).GetString());
            Assert.Equal(10.5, ws.Cell(2, 7).GetDouble());
        }
        finally { File.Delete(path); }
    }
}