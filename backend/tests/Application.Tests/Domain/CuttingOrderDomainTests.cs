using FluentAssertions;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Cutting;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Application.Tests.Domain;

public class CuttingOrderDomainTests
{
    private static CuttingOrder MakeOrder(CuttingOrderStatus targetStatus)
    {
        var roll = FabricRoll.Create(Guid.NewGuid(), Guid.NewGuid(), 10m, 200m);
        var order = CuttingOrder.Create(1, [(roll.Id, new Dictionary<string, int> { ["P"] = 5 })]);

        if (targetStatus == CuttingOrderStatus.Draft) return order;

        order.MarkSent();
        if (targetStatus == CuttingOrderStatus.SentToCutter) return order;

        order.MarkDelivered();
        if (targetStatus == CuttingOrderStatus.Delivered) return order;

        order.MarkSewingDelivered();
        return order;
    }

    [Fact]
    public void Cancel_Draft_SetsStatusCancelled()
    {
        var order = MakeOrder(CuttingOrderStatus.Draft);
        order.Cancel();
        order.Status.Should().Be(CuttingOrderStatus.Cancelled);
        order.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Cancel_SentToCutter_SetsStatusCancelled()
    {
        var order = MakeOrder(CuttingOrderStatus.SentToCutter);
        order.Cancel();
        order.Status.Should().Be(CuttingOrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_Delivered_ThrowsDomainException()
    {
        var order = MakeOrder(CuttingOrderStatus.Delivered);
        var act = () => order.Cancel();
        act.Should().Throw<DomainException>().WithMessage("*entregues*");
    }

    [Fact]
    public void Cancel_SewingDelivered_ThrowsDomainException()
    {
        var order = MakeOrder(CuttingOrderStatus.SewingDelivered);
        var act = () => order.Cancel();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CancelDelivered_AnyStatus_SetsStatusCancelled()
    {
        var order = MakeOrder(CuttingOrderStatus.SewingDelivered);
        order.CancelDelivered();
        order.Status.Should().Be(CuttingOrderStatus.Cancelled);
    }

    [Fact]
    public void FabricRoll_RevertToAvailable_FromConsumed_Succeeds()
    {
        var roll = FabricRoll.Create(Guid.NewGuid(), Guid.NewGuid(), 10m, 200m);
        roll.StartCutting();
        roll.MarkConsumed();
        roll.RevertToAvailable();
        roll.Status.Should().Be(FabricRollStatus.Available);
    }

    [Fact]
    public void FabricRoll_RevertToAvailable_FromAvailable_ThrowsDomainException()
    {
        var roll = FabricRoll.Create(Guid.NewGuid(), Guid.NewGuid(), 10m, 200m);
        var act = () => roll.RevertToAvailable();
        act.Should().Throw<DomainException>();
    }
}
