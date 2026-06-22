using MediatR;
using SistemaTraction.Application.Stock.DTOs;

namespace SistemaTraction.Application.Stock.Queries.GetShirtStock;

public record GetShirtStockQuery(string ModelCode = "REG") : IRequest<ShirtStockGridDto>;
