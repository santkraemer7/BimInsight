using BimInsight.Core.Models;
using BimInsight.Quantities;
using Xunit;

namespace BimInsight.Tests;

public class QuantityCalculatorTests
{
    [Fact]
    public void Agrupa_e_soma_por_pavimento()
    {
        var elements = new[]
        {
            new BimElement("1", "IfcWall", "P1", "Parede 20", "Térreo", "Concreto", 10, 2),
            new BimElement("2", "IfcWall", "P2", "Parede 20", "Térreo", "Concreto", 5, 1),
            new BimElement("3", "IfcSlab", "L1", "Laje 15",   "1º Pav", "Concreto", 20, 3),
        };

        var rows = QuantityCalculator.Calculate(elements, GroupMode.Storey);

        var terreo = rows.Single(r => r.Group == "Térreo");
        Assert.Equal(2, terreo.Count);
        Assert.Equal(15, terreo.TotalArea);
        Assert.Equal(3, terreo.TotalVolume);
    }
}