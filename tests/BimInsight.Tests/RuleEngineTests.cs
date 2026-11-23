using BimInsight.Core.Models;
using BimInsight.Rules;
using Xunit;

namespace BimInsight.Tests;

public class RuleEngineTests
{
    private static BimElement Wall(string id, string? material) =>
        new(id, "IFCWALL", "Parede", "Parede 20", "Térreo", material, 10, 2);

    private static RuleSet Rules(params RuleDefinition[] rules) => new(rules);

    [Fact]
    public void Reporta_parede_sem_material()
    {
        var rules = Rules(new RuleDefinition(
            "R1", "Paredes devem ter material", "IfcWall",
            CheckType.HasMaterial, Severity.Error));

        var result = RuleEngine.Evaluate(
            new[] { Wall("1", "Concreto"), Wall("2", null) }, rules);

        var violation = Assert.Single(result);
        Assert.Equal("2", violation.GlobalId);
        Assert.Equal(Severity.Error, violation.Severity);
    }

    [Fact]
    public void Ignora_elementos_de_outros_tipos()
    {
        var rules = Rules(new RuleDefinition(
            "R1", "Janelas devem ter material", "IfcWindow",
            CheckType.HasMaterial));

        var result = RuleEngine.Evaluate(new[] { Wall("1", null) }, rules);

        Assert.Empty(result);
    }

    [Fact]
    public void Curinga_aplica_a_todos_os_tipos()
    {
        var rules = Rules(new RuleDefinition(
            "R1", "Todos devem ter material", "*", CheckType.HasMaterial));

        var result = RuleEngine.Evaluate(
            new[] { Wall("1", null), Wall("2", null) }, rules);

        Assert.Equal(2, result.Count);
    }
}