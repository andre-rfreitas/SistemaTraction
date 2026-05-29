using MediatR;

namespace SistemaTraction.Application.Dtf.Commands.DeleteDtfModel;

public record DeleteDtfModelCommand(Guid Id) : IRequest;
