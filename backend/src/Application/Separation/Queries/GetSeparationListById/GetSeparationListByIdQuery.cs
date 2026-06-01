using MediatR;
using SistemaTraction.Application.Separation.DTOs;

namespace SistemaTraction.Application.Separation.Queries.GetSeparationListById;

public record GetSeparationListByIdQuery(Guid Id) : IRequest<SeparationListDetailDto?>;
