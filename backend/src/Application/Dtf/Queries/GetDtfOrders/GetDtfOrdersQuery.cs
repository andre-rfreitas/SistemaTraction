using MediatR;
using SistemaTraction.Application.Dtf.DTOs;

namespace SistemaTraction.Application.Dtf.Queries.GetDtfOrders;

public record GetDtfOrdersQuery(string? Status) : IRequest<List<DtfOrderDto>>;
