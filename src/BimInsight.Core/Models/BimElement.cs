namespace BimInsight.Core.Models;

public record BimElement(
    string GlobalId,
    string IfcType,
    string? Name,
    string? TypeName,
    string? Storey,
    string? Material,
    double? Area,
    double? Volume);