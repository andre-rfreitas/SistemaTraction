using MediatR;

namespace SistemaTraction.Application.Dtf.Commands.CancelDtfOrder;

public record CancelDtfOrderCommand(Guid Id) : IRequest;
