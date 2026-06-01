using MediatR;
using SistemaTraction.Application.Separation.DTOs;

namespace SistemaTraction.Application.Separation.Queries.GetStockCheck;

public record GetStockCheckQuery(Guid SeparationListId) : IRequest<StockCheckResultDto>;
