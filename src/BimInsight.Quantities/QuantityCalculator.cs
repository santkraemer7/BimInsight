using BimInsight.Core.Models;

namespace BimInsight.Quantities;

public enum GroupMode { IfcType, Storey, TypeName, Material }

public record QuantityRow(string Group, int Count, double TotalArea, double TotalVolume);

public static class QuantityCalculator
{
    public static IReadOnlyList<QuantityRow> Calculate(
        IEnumerable<BimElement> elements, GroupMode mode)
    {
        Func<BimElement, string?> key = mode switch
        {
            GroupMode.IfcType  => e => e.IfcType,
            GroupMode.Storey   => e => e.Storey,
            GroupMode.TypeName => e => e.TypeName,
            GroupMode.Material => e => e.Material,
            _ => e => e.IfcType
        };

        return elements
            .GroupBy(e => key(e) ?? "(não definido)")
            .Select(g => new QuantityRow(
                g.Key,
                g.Count(),
                Math.Round(g.Sum(e => e.Area ?? 0), 2),
                Math.Round(g.Sum(e => e.Volume ?? 0), 2)))
            .OrderByDescending(r => r.Count)
            .ToList();
    }
}