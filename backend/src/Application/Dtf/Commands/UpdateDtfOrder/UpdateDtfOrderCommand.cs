using MediatR;

namespace SistemaTraction.Application.Dtf.Commands.UpdateDtfOrder;

public record UpdateDtfOrderItemInput(Guid DtfModelId, int SheetQuantity);

public record UpdateDtfOrderCommand(
    Guid Id,
    string? Notes,
    List<UpdateDtfOrderItemInput> Items) : IRequest;
