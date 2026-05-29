using MediatR;
using SistemaTraction.Application.Dtf.DTOs;

namespace SistemaTraction.Application.Dtf.Queries.GetDtfStockItemByModel;

public record GetDtfStockItemByModelQuery(Guid DtfModelId) : IRequest<DtfStockItemDetailDto?>;
