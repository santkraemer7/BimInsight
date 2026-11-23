using BimInsight.Core;
using BimInsight.Quantities;
using Spectre.Console;

if (args.Length == 0 || !File.Exists(args[0]))
{
    AnsiConsole.MarkupLine("[red]Uso:[/] BimInsight.Cli <arquivo.ifc> [[ifc|storey|type|material]]");
    return 1;
}

var mode = (args.Length > 1 ? args[1] : "ifc").ToLowerInvariant() switch
{
    "storey"   => GroupMode.Storey,
    "type"     => GroupMode.TypeName,
    "material" => GroupMode.Material,
    _          => GroupMode.IfcType
};

var elements = AnsiConsole.Status().Start("Lendo modelo IFC...",
    _ => new IfcModelReader().Read(args[0]));

var rows = QuantityCalculator.Calculate(elements, mode);

var table = new Table().Title($"Quantitativos por [yellow]{mode}[/]  ({elements.Count} elementos)");
table.AddColumn("Grupo");
table.AddColumn(new TableColumn("Qtd").RightAligned());
table.AddColumn(new TableColumn("Área (m²)").RightAligned());
table.AddColumn(new TableColumn("Volume (m³)").RightAligned());

foreach (var r in rows)
    table.AddRow(Markup.Escape(r.Group), r.Count.ToString(),
                 r.TotalArea.ToString("N2"), r.TotalVolume.ToString("N2"));

AnsiConsole.Write(table);
return 0;