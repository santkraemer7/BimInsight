using BimInsight.Core.Models;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

namespace BimInsight.Core;

public sealed class IfcModelReader
{
    public IReadOnlyList<BimElement> Read(string path)
    {
        using var model = IfcStore.Open(path);

        return model.Instances
            .OfType<IIfcElement>()
            .Where(e => e is not IIfcFeatureElement) // ignora aberturas (vãos)
            .Select(ToBimElement)
            .ToList();
    }

    private static BimElement ToBimElement(IIfcElement e)
    {
        var (area, volume) = ReadQuantities(e);

        return new BimElement(
            GlobalId: e.GlobalId.ToString(),
            IfcType: e.ExpressType.ExpressName,
            Name: e.Name?.ToString(),
            TypeName: e.IsTypedBy.FirstOrDefault()?.RelatingType?.Name?.ToString(),
            Storey: (e.ContainedInStructure.FirstOrDefault()?.RelatingStructure as IIfcBuildingStorey)?.Name?.ToString(),
            Material: ReadMaterial(e),
            Area: area,
            Volume: volume);
    }

    private static (double? area, double? volume) ReadQuantities(IIfcElement e)
    {
        double? area = null, volume = null;

        var quantities = e.IsDefinedBy
            .Select(r => r.RelatingPropertyDefinition)
            .OfType<IIfcElementQuantity>()
            .SelectMany(q => q.Quantities);

        foreach (var q in quantities)
        {
            var isNet = q.Name.ToString().StartsWith("Net", StringComparison.OrdinalIgnoreCase);

            switch (q)
            {
                case IIfcQuantityVolume v when volume is null || isNet:
                    volume = (double)v.VolumeValue;
                    break;
                case IIfcQuantityArea a when area is null || isNet:
                    area = (double)a.AreaValue;
                    break;
            }
        }
        return (area, volume);
    }

    private static string? ReadMaterial(IIfcElement e)
    {
        var m = e.HasAssociations.OfType<IIfcRelAssociatesMaterial>()
                 .FirstOrDefault()?.RelatingMaterial;

        return m switch
        {
            IIfcMaterial mat => mat.Name.ToString(),
            IIfcMaterialLayerSetUsage u => u.ForLayerSet?.MaterialLayers.FirstOrDefault()?.Material?.Name.ToString(),
            IIfcMaterialLayerSet s => s.MaterialLayers.FirstOrDefault()?.Material?.Name.ToString(),
            IIfcMaterialList l => l.Materials.FirstOrDefault()?.Name.ToString(),
            IIfcMaterialConstituentSet c => c.MaterialConstituents?.FirstOrDefault()?.Material?.Name.ToString(),
            _ => null
        };
    }
}