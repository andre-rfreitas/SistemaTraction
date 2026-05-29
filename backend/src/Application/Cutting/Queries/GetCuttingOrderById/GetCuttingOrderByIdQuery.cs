using MediatR;
using SistemaTraction.Application.Cutting.DTOs;

namespace SistemaTraction.Application.Cutting.Queries.GetCuttingOrderById;

public record GetCuttingOrderByIdQuery(Guid Id) : IRequest<CuttingOrderDto?>;
