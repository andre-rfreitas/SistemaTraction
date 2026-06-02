using MediatR;

namespace SistemaTraction.Application.Separation.Commands.DeleteSeparationList;

public record DeleteSeparationListCommand(Guid Id) : IRequest;
