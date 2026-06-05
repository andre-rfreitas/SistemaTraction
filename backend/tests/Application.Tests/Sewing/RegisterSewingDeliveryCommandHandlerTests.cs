using FluentAssertions;
using SistemaTraction.Application.Sewing.Commands.RegisterSewingDelivery;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Config;
using SistemaTraction.Domain.Cutting;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Application.Tests.Sewing;

public class RegisterSewingDeliveryCommandHandlerTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly RegisterSewingDeliveryCommandHandler _handler;

    public RegisterSewingDeliveryCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new RegisterSewingDeliveryCommandHandler(_context);
        SeedConfigs();
    }

    private void SeedConfigs()
    {
        _context.AppConfigs.AddRange(
            AppConfig.Create("sewing_price_default", "5.60"),
            AppConfig.Create("sewing_price_g1", "6.30"),
            AppConfig.Create("cutting_price_default", "1.00")
        );
        _context.SaveChanges();
    }

    private async Task<(CuttingOrder order, CuttingDelivery delivery)> SeedOrderWithDelivery(
        Dictionary<string, int>? deliveredPieces = null)
    {
        var fabricType = FabricType.Create("Malha", "Regular", 20m, 15m, null);
        var fabricColor = fabricType.AddColor("Preto", "#000000");
        _context.FabricTypes.Add(fabricType);
        await _context.SaveChangesAsync();

        var roll = FabricRoll.Create(fabricType.Id, fabricColor.Id, 10m, 200m);
        _context.FabricRolls.Add(roll);
        await _context.SaveChangesAsync();

        roll.StartCutting();
        var order = CuttingOrder.Create(1, [(roll.Id, new Dictionary<string, int> { ["P"] = 10, ["M"] = 10 })]);
        _context.CuttingOrders.Add(order);
        await _context.SaveChangesAsync();

        order.MarkSent();
        await _context.SaveChangesAsync();

        var pieces = deliveredPieces ?? new Dictionary<string, int> { ["P"] = 9, ["M"] = 10 };
        var cd = CuttingDelivery.Create(order.Id, pieces, 19m);
        _context.CuttingDeliveries.Add(cd);

        order.MarkDelivered();
        roll.MarkConsumed();
        await _context.SaveChangesAsync();

        return (order, cd);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesSewingDelivery()
    {
        var (order, _) = await SeedOrderWithDelivery();
        var goodPieces = new Dictionary<string, int> { ["P"] = 8, ["M"] = 9 };
        var defectivePieces = new Dictionary<string, int> { ["P"] = 1, ["M"] = 1 };

        var command = new RegisterSewingDeliveryCommand(order.Id, goodPieces, defectivePieces);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.TotalGoodPieces.Should().Be(17);
        result.TotalDefectivePieces.Should().Be(2);
        result.SewingCostTotal.Should().Be(17 * 5.60m);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesStockItems()
    {
        var (order, _) = await SeedOrderWithDelivery();
        var goodPieces = new Dictionary<string, int> { ["P"] = 5, ["M"] = 7 };

        await _handler.Handle(
            new RegisterSewingDeliveryCommand(order.Id, goodPieces, new Dictionary<string, int>()),
            CancellationToken.None);

        var stockP = _context.StockItems.FirstOrDefault(s => s.Size == "P");
        var stockM = _context.StockItems.FirstOrDefault(s => s.Size == "M");

        stockP.Should().NotBeNull();
        stockP!.Quantity.Should().Be(5);
        stockM.Should().NotBeNull();
        stockM!.Quantity.Should().Be(7);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesFinancialEntries()
    {
        var (order, _) = await SeedOrderWithDelivery();
        var goodPieces = new Dictionary<string, int> { ["M"] = 10 };
        var defectivePieces = new Dictionary<string, int> { ["M"] = 2 };

        await _handler.Handle(
            new RegisterSewingDeliveryCommand(order.Id, goodPieces, defectivePieces),
            CancellationToken.None);

        var entries = _context.FinancialEntries.ToList();
        entries.Should().Contain(e => e.Category == "Costura");
        entries.Should().Contain(e => e.Category == "Defeitos");
    }

    [Fact]
    public async Task Handle_ValidCommand_MarksOrderSewingDelivered()
    {
        var (order, _) = await SeedOrderWithDelivery();

        await _handler.Handle(
            new RegisterSewingDeliveryCommand(order.Id,
                new Dictionary<string, int> { ["P"] = 5 },
                new Dictionary<string, int>()),
            CancellationToken.None);

        var updated = await _context.CuttingOrders.FindAsync(order.Id);
        updated!.Status.Should().Be(CuttingOrderStatus.SewingDelivered);
    }

    [Fact]
    public async Task Handle_DefectCost_CalculatesCorrectly()
    {
        // FabricRoll PriceTotal=200, delivered pieces=19 → fabricCostPerPiece=200/19≈10.526
        // cutting=1.00, sewing=5.60
        // 1 defective P → cost = (200/19 + 1.00 + 5.60) * 1
        var (order, delivery) = await SeedOrderWithDelivery(new Dictionary<string, int> { ["P"] = 9, ["M"] = 10 });
        var goodPieces = new Dictionary<string, int> { ["P"] = 8, ["M"] = 10 };
        var defectivePieces = new Dictionary<string, int> { ["P"] = 1 };

        var result = await _handler.Handle(
            new RegisterSewingDeliveryCommand(order.Id, goodPieces, defectivePieces),
            CancellationToken.None);

        var fabricCostPerPiece = 200m / 19m;
        var expectedDefectCost = 1 * (fabricCostPerPiece + 1.00m + 5.60m);
        result.DefectCostTotal.Should().BeApproximately(expectedDefectCost, 0.001m);
    }

    [Fact]
    public async Task Handle_G1SizeUsesDifferentPrice()
    {
        var (order, _) = await SeedOrderWithDelivery(new Dictionary<string, int> { ["G1"] = 10 });
        var goodPieces = new Dictionary<string, int> { ["G1"] = 10 };

        var result = await _handler.Handle(
            new RegisterSewingDeliveryCommand(order.Id, goodPieces, new Dictionary<string, int>()),
            CancellationToken.None);

        result.SewingCostTotal.Should().Be(10 * 6.30m);
    }

    [Fact]
    public async Task Handle_WrongStatus_ThrowsDomainException()
    {
        var fabricType = FabricType.Create("Malha", "Regular", 20m, 15m, null);
        var fabricColor = fabricType.AddColor("Branco", "#FFFFFF");
        _context.FabricTypes.Add(fabricType);
        await _context.SaveChangesAsync();

        var roll = FabricRoll.Create(fabricType.Id, fabricColor.Id, 5m, 100m);
        _context.FabricRolls.Add(roll);
        var order = CuttingOrder.Create(99, [(roll.Id, new Dictionary<string, int> { ["M"] = 5 })]);
        _context.CuttingOrders.Add(order);
        await _context.SaveChangesAsync();

        order.MarkSent();
        await _context.SaveChangesAsync();

        var command = new RegisterSewingDeliveryCommand(order.Id,
            new Dictionary<string, int> { ["M"] = 5 },
            new Dictionary<string, int>());

        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Entregue pelo cortador*");
    }

    [Fact]
    public async Task Handle_DuplicateDelivery_ThrowsDomainException()
    {
        var (order, _) = await SeedOrderWithDelivery();
        var cmd = new RegisterSewingDeliveryCommand(order.Id,
            new Dictionary<string, int> { ["P"] = 5 },
            new Dictionary<string, int>());

        await _handler.Handle(cmd, CancellationToken.None);

        // After first delivery the order is SewingDelivered — any attempt is rejected
        var act = () => _handler.Handle(cmd, CancellationToken.None);
        await act.Should().ThrowAsync<DomainException>();
    }

    public void Dispose() => _context.Dispose();
}
