using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaTraction.Application.Supplies.Commands.DeductSuppliesForSeparation;
using SistemaTraction.Application.Supplies.Queries.GetSupplyDeductionPreview;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.API.Controllers;

[Authorize]
[ApiController]
[Route("api/supply-deduction")]
public class SupplyDeductionController(IMediator mediator) : ControllerBase
{
    [HttpGet("preview")]
    public async Task<IActionResult> Preview([FromQuery] int orderCount, CancellationToken ct)
    {
        if (orderCount <= 0) return BadRequest(new { error = "orderCount deve ser maior que zero." });
        var result = await mediator.Send(new GetSupplyDeductionPreviewQuery(orderCount), ct);
        return Ok(result);
    }

    [HttpPost("deduct")]
    public async Task<IActionResult> Deduct([FromBody] DeductRequest request, CancellationToken ct)
    {
        try
        {
            var items = request.Items.Select(i => new DeductItem(i.SupplyStockItemId, i.Quantity)).ToList();
            await mediator.Send(new DeductSuppliesForSeparationCommand(items), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

public record DeductRequest(List<DeductRequestItem> Items);
public record DeductRequestItem(Guid SupplyStockItemId, int Quantity);
