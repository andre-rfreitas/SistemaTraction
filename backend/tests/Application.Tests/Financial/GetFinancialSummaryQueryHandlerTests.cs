using FluentAssertions;
using SistemaTraction.Application.Financial.Commands.ReverseFinancialEntry;
using SistemaTraction.Application.Financial.Queries.GetFinancialSummary;
using SistemaTraction.Domain.Financial;
using SistemaTraction.Domain.Sewing;

namespace SistemaTraction.Application.Tests.Financial;

public class GetFinancialSummaryQueryHandlerTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly GetFinancialSummaryQueryHandler _handler;

    public GetFinancialSummaryQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new GetFinancialSummaryQueryHandler(_context);
    }

    private void Seed()
    {
        _context.FinancialEntries.AddRange(
            FinancialEntry.CreateExpense(FinancialCategories.Fabric, 200m, "Bobina"),
            FinancialEntry.CreateExpense(FinancialCategories.Cutting, 50m, "Corte"),
            FinancialEntry.CreateExpense(FinancialCategories.Sewing, 280m, "Costura"),
            FinancialEntry.CreateExpense(FinancialCategories.Defects, 16m, "Defeitos"),
            FinancialEntry.CreateExpense(FinancialCategories.Dtf, 49.80m, "DTF"),
            FinancialEntry.CreateIncome("Venda", 1000m, "Vendas do mês")
        );
        // 50 peças boas produzidas no período
        _context.SewingDeliveries.Add(
            SewingDelivery.Create(Guid.NewGuid(),
                new Dictionary<string, int> { ["P"] = 25, ["M"] = 25 },
                new Dictionary<string, int>(),
                280m, 0m));
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_AggregatesCategoryTotals()
    {
        Seed();

        var result = await _handler.Handle(new GetFinancialSummaryQuery(), CancellationToken.None);

        result.TotalFabric.Should().Be(200m);
        result.TotalCutting.Should().Be(50m);
        result.TotalSewing.Should().Be(280m);
        result.TotalDefects.Should().Be(16m);
        result.TotalDtf.Should().Be(49.80m);
        result.TotalIncome.Should().Be(1000m);
    }

    [Fact]
    public async Task Handle_CalculatesBalance()
    {
        Seed();

        var result = await _handler.Handle(new GetFinancialSummaryQuery(), CancellationToken.None);

        // Income 1000 - Expense (200+50+280+16+49.80 = 595.80)
        result.TotalExpense.Should().Be(595.80m);
        result.Balance.Should().Be(404.20m);
    }

    [Fact]
    public async Task Handle_CalculatesAverageCostPerShirt()
    {
        Seed();

        var result = await _handler.Handle(new GetFinancialSummaryQuery(), CancellationToken.None);

        // (Tecido 200 + Corte 50 + Costura 280) / 50 peças = 10.60
        result.GoodPiecesProduced.Should().Be(50);
        result.AverageCostPerShirt.Should().Be(10.60m);
    }

    [Fact]
    public async Task Handle_NoProduction_AverageCostPerShirtIsNull()
    {
        _context.FinancialEntries.Add(FinancialEntry.CreateExpense(FinancialCategories.Fabric, 200m, "Bobina"));
        _context.SaveChanges();

        var result = await _handler.Handle(new GetFinancialSummaryQuery(), CancellationToken.None);

        result.AverageCostPerShirt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ReversalReducesCategoryTotal()
    {
        var fabric = FinancialEntry.CreateExpense(FinancialCategories.Fabric, 200m, "Bobina");
        _context.FinancialEntries.Add(fabric);
        _context.SaveChanges();
        await new ReverseFinancialEntryCommandHandler(_context)
            .Handle(new ReverseFinancialEntryCommand(fabric.Id), CancellationToken.None);

        var result = await _handler.Handle(new GetFinancialSummaryQuery(), CancellationToken.None);

        result.TotalFabric.Should().Be(0m);
        result.TotalExpense.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_ExpenseByCategory_IsOrderedDescending()
    {
        Seed();

        var result = await _handler.Handle(new GetFinancialSummaryQuery(), CancellationToken.None);

        result.ExpenseByCategory.First().Category.Should().Be(FinancialCategories.Sewing);
        result.ExpenseByCategory.Should().BeInDescendingOrder(c => c.Total);
    }

    public void Dispose() => _context.Dispose();
}
