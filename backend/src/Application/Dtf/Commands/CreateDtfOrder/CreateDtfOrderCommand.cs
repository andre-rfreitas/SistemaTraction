using MediatR;

namespace SistemaTraction.Application.Dtf.Commands.CreateDtfOrder;

public record CreateDtfOrderItemInput(Guid DtfModelId, int SheetQuantity);

public record CreateDtfOrderCommand(
    string? Notes,
    List<CreateDtfOrderItemInput> Items) : IRequest<Guid>;
