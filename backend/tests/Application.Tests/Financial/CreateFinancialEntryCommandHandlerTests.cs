using FluentAssertions;
using SistemaTraction.Application.Financial.Commands.CreateFinancialEntry;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Financial;

namespace SistemaTraction.Application.Tests.Financial;

public class CreateFinancialEntryCommandHandlerTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly CreateFinancialEntryCommandHandler _handler;

    public CreateFinancialEntryCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new CreateFinancialEntryCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_Income_CreatesIncomeEntry()
    {
        var command = new CreateFinancialEntryCommand("Income", "Venda", 150.50m, "Venda avulsa");

        var result = await _handler.Handle(command, CancellationToken.None);

        var entry = await _context.FinancialEntries.FindAsync(result.Id);
        entry.Should().NotBeNull();
        entry!.Type.Should().Be(FinancialEntryType.Income);
        entry.Category.Should().Be("Venda");
        entry.Amount.Should().Be(150.50m);
        entry.IsReversal.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Expense_CreatesExpenseEntry()
    {
        var command = new CreateFinancialEntryCommand("Expense", "Aluguel", 1200m, "Aluguel do galpão");

        var result = await _handler.Handle(command, CancellationToken.None);

        var entry = await _context.FinancialEntries.FindAsync(result.Id);
        entry!.Type.Should().Be(FinancialEntryType.Expense);
    }

    [Fact]
    public async Task Handle_InvalidType_ThrowsDomainException()
    {
        var command = new CreateFinancialEntryCommand("Outro", "X", 10m, "desc");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_ZeroAmount_ThrowsDomainException()
    {
        var command = new CreateFinancialEntryCommand("Expense", "X", 0m, "desc");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    public void Dispose() => _context.Dispose();
}
