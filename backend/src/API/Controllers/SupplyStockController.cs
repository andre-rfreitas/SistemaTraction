using MediatR;
using Microsoft.AspNetCore.Mvc;
using SistemaTraction.Application.Supplies.Commands.RegisterSupplyMovement;
using SistemaTraction.Application.Supplies.Queries.GetSupplyStockItems;
using SistemaTraction.Application.Supplies.Queries.GetSupplyStockMovements;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Supplies;

namespace SistemaTraction.API.Controllers;

[ApiController]
[Route("api/supply-stock")]
public class SupplyStockController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetSupplyStockItemsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/movements")]
    public async Task<IActionResult> GetMovements(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetSupplyStockMovementsQuery(id), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/movements")]
    public async Task<IActionResult> RegisterMovement(
        Guid id, [FromBody] RegisterSupplyMovementRequest request, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(
                new RegisterSupplyMovementCommand(id, request.Type, request.Quantity, request.Reason), ct);
            return Ok(result);
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

public record RegisterSupplyMovementRequest(SupplyMovementType Type, int Quantity, string? Reason);
