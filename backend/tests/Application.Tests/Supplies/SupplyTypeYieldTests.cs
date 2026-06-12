using FluentAssertions;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Supplies;

namespace SistemaTraction.Application.Tests.Supplies;

public class SupplyTypeYieldTests
{
    [Fact]
    public void SetYield_PerOrder_SetsFieldsCorrectly()
    {
        var st = SupplyType.Create("Envelope", "un");
        st.SetYield(YieldBasis.PerOrder, 1m, null);

        st.YieldBasis.Should().Be(YieldBasis.PerOrder);
        st.YieldQuantity.Should().Be(1m);
        st.YieldProductName.Should().BeNull();
    }

    [Fact]
    public void SetYield_PerProduct_SetsFieldsCorrectly()
    {
        var st = SupplyType.Create("Etiqueta cetim", "un");
        st.SetYield(YieldBasis.PerProduct, 1m, "Camiseta");

        st.YieldBasis.Should().Be(YieldBasis.PerProduct);
        st.YieldQuantity.Should().Be(1m);
        st.YieldProductName.Should().Be("Camiseta");
    }

    [Fact]
    public void SetYield_PerProduct_IgnoresProductNameForPerOrder()
    {
        var st = SupplyType.Create("Envelope", "un");
        st.SetYield(YieldBasis.PerOrder, 5m, "Camiseta");

        st.YieldProductName.Should().BeNull();
    }

    [Fact]
    public void SetYield_ZeroQuantity_ThrowsDomainException()
    {
        var st = SupplyType.Create("Linha", "rolo");
        var act = () => st.SetYield(YieldBasis.PerProduct, 0m, "Camiseta");
        act.Should().Throw<DomainException>().WithMessage("*maior que zero*");
    }

    [Fact]
    public void SetYield_PerProduct_WithoutName_ThrowsDomainException()
    {
        var st = SupplyType.Create("Linha", "rolo");
        var act = () => st.SetYield(YieldBasis.PerProduct, 20m, null);
        act.Should().Throw<DomainException>().WithMessage("*produto*");
    }

    [Fact]
    public void SetYield_PerProduct_WithWhitespaceName_ThrowsDomainException()
    {
        var st = SupplyType.Create("Linha", "rolo");
        var act = () => st.SetYield(YieldBasis.PerProduct, 20m, "   ");
        act.Should().Throw<DomainException>().WithMessage("*produto*");
    }

    [Fact]
    public void SetYield_NegativeQuantity_ThrowsDomainException()
    {
        var st = SupplyType.Create("Linha", "rolo");
        var act = () => st.SetYield(YieldBasis.PerProduct, -1m, "Camiseta");
        act.Should().Throw<DomainException>().WithMessage("*maior que zero*");
    }

    [Fact]
    public void ClearYield_ResetsAllFields()
    {
        var st = SupplyType.Create("Etiqueta", "un");
        st.SetYield(YieldBasis.PerProduct, 1m, "Camiseta");
        st.ClearYield();

        st.YieldBasis.Should().Be(YieldBasis.None);
        st.YieldQuantity.Should().BeNull();
        st.YieldProductName.Should().BeNull();
    }

    [Fact]
    public void SetYield_None_ClearsFields()
    {
        var st = SupplyType.Create("Etiqueta", "un");
        st.SetYield(YieldBasis.PerProduct, 1m, "Camiseta");
        st.SetYield(YieldBasis.None, 0m, null);

        st.YieldBasis.Should().Be(YieldBasis.None);
        st.YieldQuantity.Should().BeNull();
    }
}
