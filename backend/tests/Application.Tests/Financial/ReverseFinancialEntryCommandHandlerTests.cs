using FluentAssertions;
using SistemaTraction.Application.Financial.Commands.ReverseFinancialEntry;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Cutting;
using SistemaTraction.Domain.Fabric;
using SistemaTraction.Domain.Financial;
using SistemaTraction.Domain.Sewing;
using SistemaTraction.Domain.Stock;

namespace SistemaTraction.Application.Tests.Financial;

public class ReverseFinancialEntryCommandHandlerTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly ReverseFinancialEntryCommandHandler _handler;

    public ReverseFinancialEntryCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new ReverseFinancialEntryCommandHandler(_context);
    }

    private async Task<FinancialEntry> SeedExpense(decimal amount = 100m)
    {
        var entry = FinancialEntry.CreateExpense("Corte", amount, "Pedido #1");
        _context.FinancialEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    [Fact]
    public async Task Handle_CreatesReversalWithNegativeAmount()
    {
        var original = await SeedExpense(100m);

        var result = await _handler.Handle(new ReverseFinancialEntryCommand(original.Id), CancellationToken.None);

        var reversal = await _context.FinancialEntries.FindAsync(result.ReversalId);
        reversal.Should().NotBeNull();
        reversal!.Amount.Should().Be(-100m);
        reversal.Type.Should().Be(original.Type);
        reversal.Category.Should().Be(original.Category);
        reversal.ReferenceId.Should().Be(original.Id);
        reversal.IsReversal.Should().BeTrue();
        reversal.Description.Should().StartWith("Estorno:");
    }

    [Fact]
    public async Task Handle_ReversalNetsCategoryToZero()
    {
        var original = await SeedExpense(100m);

        await _handler.Handle(new ReverseFinancialEntryCommand(original.Id), CancellationToken.None);

        var total = _context.FinancialEntries
            .Where(e => e.Category == "Corte")
            .Sum(e => e.Amount);
        total.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_AlreadyReversed_ThrowsDomainException()
    {
        var original = await SeedExpense();
        await _handler.Handle(new ReverseFinancialEntryCommand(original.Id), CancellationToken.None);

        var act = () => _handler.Handle(new ReverseFinancialEntryCommand(original.Id), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*já foi estornado*");
    }

    [Fact]
    public async Task Handle_ReversingAReversal_ThrowsDomainException()
    {
        var original = await SeedExpense();
        var result = await _handler.Handle(new ReverseFinancialEntryCommand(original.Id), CancellationToken.None);

        var act = () => _handler.Handle(new ReverseFinancialEntryCommand(result.ReversalId), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_NonExistentEntry_ThrowsDomainException()
    {
        var act = () => _handler.Handle(new ReverseFinancialEntryCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*não encontrado*");
    }

    private async Task<(FinancialEntry entry, CuttingOrder order, FabricRoll roll)> SeedCuttingDeliveryEntry()
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
        await _context.SaveChangesAsync();

        var delivery = CuttingDelivery.Create(order.Id, new Dictionary<string, int> { ["P"] = 5 }, 5m);
        _context.CuttingDeliveries.Add(delivery);
        order.MarkDelivered();
        roll.MarkConsumed();
        await _context.SaveChangesAsync();

        var entry = FinancialEntry.CreateExpense("Corte", 5m, "Corte Pedido #1", delivery.Id, "CuttingDelivery");
        _context.FinancialEntries.Add(entry);
        await _context.SaveChangesAsync();

        return (entry, order, roll);
    }

    [Fact]
    public async Task Handle_CuttingDelivery_CancelsCuttingOrderAndRevertsRoll()
    {
        var (entry, order, roll) = await SeedCuttingDeliveryEntry();

        await _handler.Handle(new ReverseFinancialEntryCommand(entry.Id), CancellationToken.None);

        var updatedOrder = await _context.CuttingOrders.FindAsync(order.Id);
        var updatedRoll = await _context.FabricRolls.FindAsync(roll.Id);

        updatedOrder!.Status.Should().Be(CuttingOrderStatus.Cancelled);
        updatedRoll!.Status.Should().Be(FabricRollStatus.Available);
    }

    [Fact]
    public async Task Handle_CuttingDelivery_CreatesReversalEntry()
    {
        var (entry, _, _) = await SeedCuttingDeliveryEntry();

        await _handler.Handle(new ReverseFinancialEntryCommand(entry.Id), CancellationToken.None);

        var reversal = _context.FinancialEntries.SingleOrDefault(e => e.IsReversal && e.ReferenceId == entry.Id);
        reversal.Should().NotBeNull();
        reversal!.Amount.Should().Be(-5m);
    }

    [Fact]
    public async Task Handle_CuttingDelivery_WithSewingDelivery_ThrowsDomainException()
    {
        var (entry, order, _) = await SeedCuttingDeliveryEntry();

        var sewingDelivery = SewingDelivery.Create(order.Id, new Dictionary<string, int> { ["P"] = 4 },
            new Dictionary<string, int>(), 22.4m, 0m);
        _context.SewingDeliveries.Add(sewingDelivery);
        order.MarkSewingDelivered();
        await _context.SaveChangesAsync();

        var act = () => _handler.Handle(new ReverseFinancialEntryCommand(entry.Id), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*costura*");
    }

    private async Task<(FinancialEntry sewingEntry, CuttingOrder order, Guid stockItemId)> SeedSewingDeliveryEntry()
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

        var stockItem = StockItem.Create(fabricColor.Id, "Preto", "Malha", "Regular", "P", 9, "REG");
        _context.StockItems.Add(stockItem);
        await _context.SaveChangesAsync();

        _context.ShirtStockMovements.Add(ShirtStockMovement.Create(
            stockItem.Id, fabricColor.Id, "Preto", "P", 9,
            $"Costura pedido #{order.OrderNumber}", "Costureiro", order.Id));

        order.MarkSewingDelivered();
        await _context.SaveChangesAsync();

        var sewingEntry = FinancialEntry.CreateExpense("Costura", 50.4m, "Costura pedido #2 — 9 peças",
            sewingDelivery.Id, "SewingDelivery");
        var defectEntry = FinancialEntry.CreateExpense("Defeitos", 27m, "Defeitos pedido #2 — 1 peça",
            sewingDelivery.Id, "SewingDelivery");
        _context.FinancialEntries.AddRange(sewingEntry, defectEntry);
        await _context.SaveChangesAsync();

        return (sewingEntry, order, stockItem.Id);
    }

    [Fact]
    public async Task Handle_SewingDelivery_CancelsCuttingOrder()
    {
        var (sewingEntry, order, _) = await SeedSewingDeliveryEntry();
        await _handler.Handle(new ReverseFinancialEntryCommand(sewingEntry.Id), CancellationToken.None);
        var updatedOrder = await _context.CuttingOrders.FindAsync(order.Id);
        updatedOrder!.Status.Should().Be(CuttingOrderStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_SewingDelivery_RevertsShirtStock()
    {
        var (sewingEntry, _, stockItemId) = await SeedSewingDeliveryEntry();
        await _handler.Handle(new ReverseFinancialEntryCommand(sewingEntry.Id), CancellationToken.None);
        var stockItem = await _context.StockItems.FindAsync(stockItemId);
        stockItem!.Quantity.Should().Be(0);
    }

    [Fact]
    public async Task Handle_SewingDelivery_ReversesAllLinkedFinancialEntries()
    {
        var (sewingEntry, _, _) = await SeedSewingDeliveryEntry();
        await _handler.Handle(new ReverseFinancialEntryCommand(sewingEntry.Id), CancellationToken.None);
        var reversals = _context.FinancialEntries.Where(e => e.IsReversal).ToList();
        reversals.Should().HaveCount(2);
        reversals.Select(r => r.Amount).Should().Contain(-50.4m);
        reversals.Select(r => r.Amount).Should().Contain(-27m);
    }

    [Fact]
    public async Task Handle_SewingDelivery_CreatesCompensatingStockMovement()
    {
        var (sewingEntry, order, _) = await SeedSewingDeliveryEntry();
        await _handler.Handle(new ReverseFinancialEntryCommand(sewingEntry.Id), CancellationToken.None);
        var compensating = _context.ShirtStockMovements
            .Where(m => m.ReferenceId == order.Id && m.Delta < 0)
            .ToList();
        compensating.Should().HaveCount(1);
        compensating[0].Delta.Should().Be(-9);
        compensating[0].Origin.Should().Be("Estorno");
    }

    public void Dispose() => _context.Dispose();
}
