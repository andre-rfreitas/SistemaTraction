using FluentAssertions;
using MediatR;
using NSubstitute;
using SistemaTraction.Application.Cutting.Commands.CancelCuttingOrder;
using SistemaTraction.Application.Financial.Commands.ReverseFinancialEntry;
using SistemaTraction.Domain.Cutting;
using SistemaTraction.Domain.Fabric;
using SistemaTraction.Domain.Financial;
using SistemaTraction.Domain.Sewing;
using SistemaTraction.Domain.Stock;

namespace SistemaTraction.Application.Tests.Cutting;

public class CancelCuttingOrderCommandHandlerTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly ISender _sender;
    private readonly CancelCuttingOrderCommandHandler _handler;

    public CancelCuttingOrderCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _sender = Substitute.For<ISender>();
        _handler = new CancelCuttingOrderCommandHandler(_context, _sender);
    }

    private async Task<(CuttingOrder order, CuttingDelivery delivery)> SeedDeliveredOrder()
    {
        var fabricType = FabricType.Create("Malha", "Regular", 20m, 15m, null);
        var fabricColor = fabricType.AddColor("Preto", "#000000");
        _context.FabricTypes.Add(fabricType);
        await _context.SaveChangesAsync();

        var roll = FabricRoll.Create(fabricType.Id, fabricColor.Id, 10m, 200m);
        _context.FabricRolls.Add(roll);
        roll.StartCutting();
        await _context.SaveChangesAsync();

        var order = CuttingOrder.Create(1, [(roll.Id, new Dictionary<string, int> { ["P"] = 5 })]);
        _context.CuttingOrders.Add(order);
        await _context.SaveChangesAsync();

        order.MarkSent();
        var delivery = CuttingDelivery.Create(order.Id, new Dictionary<string, int> { ["P"] = 5 }, 5m);
        _context.CuttingDeliveries.Add(delivery);
        order.MarkDelivered();
        roll.MarkConsumed();
        await _context.SaveChangesAsync();

        return (order, delivery);
    }

    private async Task<(CuttingOrder order, SewingDelivery delivery, Guid stockItemId)> SeedSewingDeliveredOrder()
    {
        var fabricType = FabricType.Create("Malha", "Regular", 20m, 15m, null);
        var fabricColor = fabricType.AddColor("Preto", "#000000");
        _context.FabricTypes.Add(fabricType);
        await _context.SaveChangesAsync();

        var roll = FabricRoll.Create(fabricType.Id, fabricColor.Id, 10m, 200m);
        _context.FabricRolls.Add(roll);
        roll.StartCutting();
        await _context.SaveChangesAsync();

        var order = CuttingOrder.Create(2, [(roll.Id, new Dictionary<string, int> { ["P"] = 10 })]);
        _context.CuttingOrders.Add(order);
        await _context.SaveChangesAsync();

        order.MarkSent();
        var cuttingDelivery = CuttingDelivery.Create(order.Id, new Dictionary<string, int> { ["P"] = 10 }, 10m);
        _context.CuttingDeliveries.Add(cuttingDelivery);
        order.MarkDelivered();
        roll.MarkConsumed();
        await _context.SaveChangesAsync();

        var sewingDelivery = SewingDelivery.Create(order.Id,
            new Dictionary<string, int> { ["P"] = 9 },
            new Dictionary<string, int> { ["P"] = 1 },
            50.4m, 27m);
        _context.SewingDeliveries.Add(sewingDelivery);

        var stockItem = StockItem.Create(fabricColor.Id, "Preto", "Malha", "Regular", "P", 9);
        _context.StockItems.Add(stockItem);
        _context.ShirtStockMovements.Add(ShirtStockMovement.Create(
            stockItem.Id, fabricColor.Id, "Preto", "P", 9,
            "Costura pedido #2", "Costureiro", order.Id));

        order.MarkSewingDelivered();
        await _context.SaveChangesAsync();

        return (order, sewingDelivery, stockItem.Id);
    }

    [Fact]
    public async Task Handle_Delivered_WithFinancialEntry_DispatchesReverseCommand()
    {
        var (order, delivery) = await SeedDeliveredOrder();
        var entry = FinancialEntry.CreateExpense("Corte", 5m, "Corte #1", delivery.Id, "CuttingDelivery");
        _context.FinancialEntries.Add(entry);
        await _context.SaveChangesAsync();

        await _handler.Handle(new CancelCuttingOrderCommand(order.Id), CancellationToken.None);

        await _sender.Received(1).Send(
            Arg.Is<ReverseFinancialEntryCommand>(c => c.Id == entry.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Delivered_WithoutFinancialEntry_CancelsDirectly()
    {
        var (order, _) = await SeedDeliveredOrder();

        await _handler.Handle(new CancelCuttingOrderCommand(order.Id), CancellationToken.None);

        await _sender.DidNotReceive().Send(
            Arg.Any<ReverseFinancialEntryCommand>(),
            Arg.Any<CancellationToken>());
        var updated = await _context.CuttingOrders.FindAsync(order.Id);
        updated!.Status.Should().Be(CuttingOrderStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_SewingDelivered_WithFinancialEntry_DispatchesReverseCommand()
    {
        var (order, sewingDelivery, _) = await SeedSewingDeliveredOrder();
        var entry = FinancialEntry.CreateExpense("Costura", 50.4m, "Costura #2", sewingDelivery.Id, "SewingDelivery");
        _context.FinancialEntries.Add(entry);
        await _context.SaveChangesAsync();

        await _handler.Handle(new CancelCuttingOrderCommand(order.Id), CancellationToken.None);

        await _sender.Received(1).Send(
            Arg.Is<ReverseFinancialEntryCommand>(c => c.Id == entry.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SewingDelivered_WithoutFinancialEntry_CancelsDirectly()
    {
        var (order, _, _) = await SeedSewingDeliveredOrder();

        await _handler.Handle(new CancelCuttingOrderCommand(order.Id), CancellationToken.None);

        await _sender.DidNotReceive().Send(
            Arg.Any<ReverseFinancialEntryCommand>(),
            Arg.Any<CancellationToken>());
        var updated = await _context.CuttingOrders.FindAsync(order.Id);
        updated!.Status.Should().Be(CuttingOrderStatus.Cancelled);
    }

    public void Dispose() => _context.Dispose();
}
