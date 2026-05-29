using MediatR;

namespace SistemaTraction.Application.Dtf.Commands.UpdateDtfModel;

public record UpdateDtfModelCommand(
    Guid Id,
    string Name,
    string SheetLabel,
    int StampsPerSheet,
    decimal SheetCost
) : IRequest;
