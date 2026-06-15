using MediatR;

namespace SistemaTraction.Application.Dtf.Commands.ReceiveDtfOrder;

public record ReceiveDtfOrderCommand(Guid Id) : IRequest;
