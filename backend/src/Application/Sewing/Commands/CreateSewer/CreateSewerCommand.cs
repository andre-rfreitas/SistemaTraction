using MediatR;
using SistemaTraction.Application.Sewing.DTOs;

namespace SistemaTraction.Application.Sewing.Commands.CreateSewer;

public record ProductTypeInput(string Name, decimal PriceDefault, decimal PriceG1);

public record CreateSewerCommand(string Name, string? Phone, List<ProductTypeInput> ProductTypes) : IRequest<SewerDto>;
