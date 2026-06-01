using MediatR;

namespace SistemaTraction.Application.Separation.Commands.DeleteSkuCode;

public record DeleteSkuCodeCommand(Guid Id) : IRequest;
