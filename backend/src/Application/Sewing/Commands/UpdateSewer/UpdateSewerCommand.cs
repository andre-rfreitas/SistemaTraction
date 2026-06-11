using MediatR;
using SistemaTraction.Application.Sewing.Commands.CreateSewer;
using SistemaTraction.Application.Sewing.DTOs;

namespace SistemaTraction.Application.Sewing.Commands.UpdateSewer;

public record UpdateSewerCommand(Guid Id, string Name, string? Phone, List<ProductTypeInput> ProductTypes) : IRequest<SewerDto>;
