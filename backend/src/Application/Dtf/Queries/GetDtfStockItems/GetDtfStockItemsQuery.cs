using MediatR;
using SistemaTraction.Application.Dtf.DTOs;

namespace SistemaTraction.Application.Dtf.Queries.GetDtfStockItems;

public record GetDtfStockItemsQuery : IRequest<List<DtfStockItemDto>>;
