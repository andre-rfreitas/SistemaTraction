using MediatR;

namespace SistemaTraction.Application.Stock.Commands.CreateGenericProduct;

public record CreateGenericProductCommand(
    Guid CategoryId,
    string Name,
    int InitialQuantity,
    decimal UnitCost = 0m,
    string Reason = "Cadastro inicial"
) : IRequest<Guid>;
