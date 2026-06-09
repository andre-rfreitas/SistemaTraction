using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaTraction.Application.Supplies.Commands.UpsertSupplyOrderConfig;
using SistemaTraction.Application.Supplies.Queries.GetSupplyOrderConfigs;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.API.Controllers;

[Authorize]
[ApiController]
[Route("api/supply-order-configs")]
public class SupplyOrderConfigsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetSupplyOrderConfigsQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Upsert([FromBody] UpsertSupplyOrderConfigRequest request, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new UpsertSupplyOrderConfigCommand(request.SupplyTypeId, request.QuantityPerOrder), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

public record UpsertSupplyOrderConfigRequest(Guid SupplyTypeId, int QuantityPerOrder);
