using MediatR;
using SistemaTraction.Application.Cutting.DTOs;

namespace SistemaTraction.Application.Cutting.Queries.GetCuttingOrders;

public record GetCuttingOrdersQuery(string? Status = null) : IRequest<List<CuttingOrderDto>>;
