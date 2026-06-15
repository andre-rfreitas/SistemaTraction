using MediatR;

namespace SistemaTraction.Application.Dtf.Commands.SendDtfOrder;

public record SendDtfOrderCommand(Guid Id) : IRequest;
