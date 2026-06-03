using MediatR;
using SistemaTraction.Application.Stock.DTOs;

namespace SistemaTraction.Application.Stock.Queries.GetShirtStockMovements;

public record GetShirtStockMovementsQuery(int Page = 1, int PageSize = 20)
    : IRequest<ShirtStockMovementsDto>;
