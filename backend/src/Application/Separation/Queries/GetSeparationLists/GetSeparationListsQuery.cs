using MediatR;
using SistemaTraction.Application.Separation.DTOs;

namespace SistemaTraction.Application.Separation.Queries.GetSeparationLists;

public record GetSeparationListsQuery : IRequest<List<SeparationListSummaryDto>>;
