using BimInsight.Core;
using BimInsight.Quantities;
using BimInsight.Rules;
using Spectre.Console;

if (args.Length == 0)
{
    PrintUsage();
    return 1;
}

return args[0].Equals("check", StringComparison.OrdinalIgnoreCase)
    ? RunCheck(args)
    : RunQuantities(args);

static void PrintUsage()
{
    AnsiConsole.MarkupLine("[red]Uso:[/]");
    AnsiConsole.MarkupLine("  BimInsight.Cli <arquivo.ifc> [[ifc|storey|type|material]]");
    AnsiConsole.MarkupLine("  BimInsight.Cli check <arquivo.ifc> <regras.json>");
}

static int RunQuantities(string[] args)
{
    if (!File.Exists(args[0]))
    {
        PrintUsage();
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
}

static int RunCheck(string[] args)
{
    if (args.Length < 3 || !File.Exists(args[1]) || !File.Exists(args[2]))
    {
        PrintUsage();
        return 1;
    }

    var elements = AnsiConsole.Status().Start("Lendo modelo IFC...",
        _ => new IfcModelReader().Read(args[1]));

    var ruleSet = RuleLoader.Load(args[2]);
    var violations = RuleEngine.Evaluate(elements, ruleSet);

    if (violations.Count == 0)
    {
        AnsiConsole.MarkupLine($"[green]Nenhuma violação.[/] {elements.Count} elementos verificados.");
        return 0;
    }

    var table = new Table().Title($"Violações encontradas ({violations.Count})");
    table.AddColumn("Severidade");
    table.AddColumn("Regra");
    table.AddColumn("Descrição");
    table.AddColumn("Tipo");
    table.AddColumn("Elemento");

    foreach (var v in violations.OrderByDescending(v => v.Severity))
    {
        var color = v.Severity switch
        {
            Severity.Error   => "red",
            Severity.Warning => "yellow",
            _                => "blue"
        };

        table.AddRow(
            $"[{color}]{v.Severity}[/]",
            v.RuleId,
            Markup.Escape(v.Description),
            Markup.Escape(v.IfcType),
            Markup.Escape(v.ElementName ?? v.GlobalId));
    }

    AnsiConsole.Write(table);

    var errors = violations.Count(v => v.Severity == Severity.Error);
    AnsiConsole.MarkupLine(
        $"\n[red]{errors} erro(s)[/], " +
        $"[yellow]{violations.Count(v => v.Severity == Severity.Warning)} aviso(s)[/], " +
        $"[blue]{violations.Count(v => v.Severity == Severity.Info)} info[/]");

    // código 2 quando há erros: útil para integrar com CI
    return errors > 0 ? 2 : 0;
}