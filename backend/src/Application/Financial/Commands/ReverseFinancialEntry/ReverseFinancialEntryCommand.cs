using MediatR;

namespace SistemaTraction.Application.Financial.Commands.ReverseFinancialEntry;

public record ReverseFinancialEntryCommand(Guid Id) : IRequest<ReverseFinancialEntryResult>;

public record ReverseFinancialEntryResult(Guid ReversalId);
