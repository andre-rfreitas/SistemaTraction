using FluentAssertions;
using SistemaTraction.Application.Financial.Commands.ReverseFinancialEntry;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Financial;

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

    public void Dispose() => _context.Dispose();
}
