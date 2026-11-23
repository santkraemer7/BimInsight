namespace BimInsight.Rules;

public enum Severity { Info, Warning, Error }

public enum CheckType
{
    HasName,
    HasTypeName,
    HasMaterial,
    HasStorey,
    HasArea,
    HasVolume
}

public record RuleDefinition(
    string Id,
    string Description,
    string IfcType,
    CheckType Check,
    Severity Severity = Severity.Warning);

public record RuleSet(IReadOnlyList<RuleDefinition> Rules);

public record Violation(
    string RuleId,
    string Description,
    Severity Severity,
    string GlobalId,
    string IfcType,
    string? ElementName);