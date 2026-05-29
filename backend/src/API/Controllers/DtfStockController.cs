using MediatR;
using Microsoft.AspNetCore.Mvc;
using SistemaTraction.Application.Dtf.Commands.RegisterDtfMovement;
using SistemaTraction.Application.Dtf.Queries.GetDtfStockItemByModel;
using SistemaTraction.Application.Dtf.Queries.GetDtfStockItems;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.API.Controllers;

[ApiController]
[Route("api/dtf-stock")]
public class DtfStockController(IMediator mediator) : ControllerBase
{
    // GET api/dtf-stock
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetDtfStockItemsQuery(), ct);
        return Ok(result);
    }

    // GET api/dtf-stock/{dtfModelId}
    [HttpGet("{dtfModelId:guid}")]
    public async Task<IActionResult> GetByModel(Guid dtfModelId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetDtfStockItemByModelQuery(dtfModelId), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // POST api/dtf-stock/movements
    [HttpPost("movements")]
    public async Task<IActionResult> RegisterMovement(
        [FromBody] RegisterMovementRequest request, CancellationToken ct)
    {
        try
        {
            var movementId = await mediator.Send(
                new RegisterDtfMovementCommand(
                    request.DtfModelId,
                    request.Type,
                    request.Quantity,
                    request.Reason), ct);

            return Ok(new { movementId });
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

public record RegisterMovementRequest(
    Guid DtfModelId,
    DtfMovementType Type,
    int Quantity,
    string? Reason
);
