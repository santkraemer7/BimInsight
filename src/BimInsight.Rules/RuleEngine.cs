using BimInsight.Core.Models;

namespace BimInsight.Rules;

public static class RuleEngine
{
    public static IReadOnlyList<Violation> Evaluate(
        IEnumerable<BimElement> elements, RuleSet ruleSet)
    {
        var all = elements.ToList();
        var violations = new List<Violation>();

        foreach (var rule in ruleSet.Rules)
        {
            var failing = all
                .Where(e => Matches(e, rule.IfcType))
                .Where(e => !Passes(e, rule.Check));

            foreach (var e in failing)
            {
                violations.Add(new Violation(
                    rule.Id, rule.Description, rule.Severity,
                    e.GlobalId, e.IfcType, e.Name));
            }
        }

        return violations;
    }

    // "*" vale para qualquer tipo; a comparação ignora maiúsculas/minúsculas
    private static bool Matches(BimElement e, string ifcType) =>
        ifcType == "*" ||
        string.Equals(e.IfcType, ifcType, StringComparison.OrdinalIgnoreCase);

    private static bool Passes(BimElement e, CheckType check) => check switch
    {
        CheckType.HasName     => !string.IsNullOrWhiteSpace(e.Name),
        CheckType.HasTypeName => !string.IsNullOrWhiteSpace(e.TypeName),
        CheckType.HasMaterial => !string.IsNullOrWhiteSpace(e.Material),
        CheckType.HasStorey   => !string.IsNullOrWhiteSpace(e.Storey),
        CheckType.HasArea     => e.Area is > 0,
        CheckType.HasVolume   => e.Volume is > 0,
        _ => true
    };
}