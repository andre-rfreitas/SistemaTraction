using MediatR;
using SistemaTraction.Application.Dtf.DTOs;

namespace SistemaTraction.Application.Dtf.Queries.GetDtfModelById;

public record GetDtfModelByIdQuery(Guid Id) : IRequest<DtfModelDto?>;
