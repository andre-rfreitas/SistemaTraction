using MediatR;
using SistemaTraction.Application.Separation.DTOs;

namespace SistemaTraction.Application.Separation.Commands.UpsertSkuCode;

public record UpsertSkuCodeCommand(Guid? Id, string Code, string Value, string Category)
    : IRequest<SkuCodeDto>;
