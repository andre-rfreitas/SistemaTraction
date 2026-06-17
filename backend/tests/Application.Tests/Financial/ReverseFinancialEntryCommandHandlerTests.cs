using FluentAssertions;
using SistemaTraction.Application.Financial.Commands.ReverseFinancialEntry;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Cutting;
using SistemaTraction.Domain.Fabric;
using SistemaTraction.Domain.Financial;
using SistemaTraction.Domain.Sewing;

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

    public void Dispose() => _context.Dispose();
}
