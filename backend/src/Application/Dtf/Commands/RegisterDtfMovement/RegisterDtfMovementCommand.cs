using MediatR;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Application.Dtf.Commands.RegisterDtfMovement;

public record RegisterDtfMovementCommand(
    Guid DtfModelId,
    DtfMovementType Type,
    int Quantity,
    string? Reason
) : IRequest<Guid>;
