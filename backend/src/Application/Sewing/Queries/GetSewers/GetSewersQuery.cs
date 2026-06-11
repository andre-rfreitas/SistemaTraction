using MediatR;
using SistemaTraction.Application.Sewing.DTOs;

namespace SistemaTraction.Application.Sewing.Queries.GetSewers;

public record GetSewersQuery(bool IncludeInactive = false) : IRequest<List<SewerDto>>;
