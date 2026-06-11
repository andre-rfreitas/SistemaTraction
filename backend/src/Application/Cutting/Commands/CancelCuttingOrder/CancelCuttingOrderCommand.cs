using MediatR;

namespace SistemaTraction.Application.Cutting.Commands.CancelCuttingOrder;

public record CancelCuttingOrderCommand(Guid OrderId) : IRequest<Unit>;
