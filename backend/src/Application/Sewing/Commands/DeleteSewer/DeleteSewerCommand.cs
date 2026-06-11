using MediatR;

namespace SistemaTraction.Application.Sewing.Commands.DeleteSewer;

public record DeleteSewerCommand(Guid Id) : IRequest;
