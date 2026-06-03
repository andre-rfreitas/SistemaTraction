using MediatR;
using Microsoft.AspNetCore.Mvc;
using SistemaTraction.Application.Stock.Commands.AdjustShirtStock;
using SistemaTraction.Application.Stock.Queries.GetShirtStock;
using SistemaTraction.Application.Stock.Queries.GetShirtStockMovements;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.API.Controllers;

[ApiController]
[Route("api/stock/shirts")]
public class ShirtStockController(IMediator mediator) : ControllerBase
{
    // GET api/stock/shirts
    [HttpGet]
    public async Task<IActionResult> GetGrid(CancellationToken ct)
    {
        var result = await mediator.Send(new GetShirtStockQuery(), ct);
        return Ok(result);
    }

    // GET api/stock/shirts/movements?page=1&pageSize=20
    [HttpGet("movements")]
    public async Task<IActionResult> GetMovements(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetShirtStockMovementsQuery(page, pageSize), ct);
        return Ok(result);
    }

    // POST api/stock/shirts/adjustment
    [HttpPost("adjustment")]
    public async Task<IActionResult> Adjust([FromBody] AdjustShirtStockRequest request, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(
                new AdjustShirtStockCommand(
                    request.FabricColorId,
                    request.Size,
                    request.AdjustmentType,
                    request.Quantity,
                    request.Reason), ct);
            return Ok(result);
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

public record AdjustShirtStockRequest(
    Guid FabricColorId,
    string Size,
    string AdjustmentType,
    int Quantity,
    string Reason
);
