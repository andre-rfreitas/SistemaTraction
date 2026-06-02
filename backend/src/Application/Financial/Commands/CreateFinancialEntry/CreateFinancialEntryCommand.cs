using MediatR;

namespace SistemaTraction.Application.Financial.Commands.CreateFinancialEntry;

public record CreateFinancialEntryCommand(
    string Type,
    string Category,
    decimal Amount,
    string Description
) : IRequest<CreateFinancialEntryResult>;

public record CreateFinancialEntryResult(Guid Id);
