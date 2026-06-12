using MediatR;

namespace SistemaTraction.Application.Sewing.Queries.GetSewerProductTypeNames;

public record GetSewerProductTypeNamesQuery : IRequest<List<string>>;
