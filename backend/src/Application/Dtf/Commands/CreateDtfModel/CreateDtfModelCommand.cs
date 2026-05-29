using MediatR;

namespace SistemaTraction.Application.Dtf.Commands.CreateDtfModel;

public record CreateDtfModelCommand(
    string Name,
    string SheetLabel,
    int StampsPerSheet,
    decimal SheetCost
) : IRequest<Guid>;
