using MediatR;

namespace SistemaTraction.Application.Stock.Commands.CreateGenericProductCategory;

public record CreateGenericProductCategoryCommand(string Name) : IRequest<Guid>;
