using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Financial;

namespace SistemaTraction.Application.Financial.Commands.ReverseFinancialEntry;

public class ReverseFinancialEntryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ReverseFinancialEntryCommand, ReverseFinancialEntryResult>
{
    public async Task<ReverseFinancialEntryResult> Handle(ReverseFinancialEntryCommand request, CancellationToken cancellationToken)
    {
        var original = await context.FinancialEntries
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new DomainException("Lançamento financeiro não encontrado.");

        var alreadyReversed = await context.FinancialEntries
            .AnyAsync(e => e.IsReversal && e.ReferenceId == original.Id, cancellationToken);

        if (alreadyReversed)
            throw new DomainException("Este lançamento já foi estornado.");

        var reversal = FinancialEntry.CreateReversal(original);
        context.FinancialEntries.Add(reversal);
        await context.SaveChangesAsync(cancellationToken);

        return new ReverseFinancialEntryResult(reversal.Id);
    }
}
