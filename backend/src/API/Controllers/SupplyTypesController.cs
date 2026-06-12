using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaTraction.Application.Supplies.Commands.CreateSupplyType;
using SistemaTraction.Application.Supplies.Commands.DeleteSupplyType;
using SistemaTraction.Application.Supplies.Commands.UpdateSupplyType;
using SistemaTraction.Application.Supplies.Queries.GetSupplyTypes;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Supplies;

namespace SistemaTraction.API.Controllers;

[Authorize]
[ApiController]
[Route("api/supply-types")]
public class SupplyTypesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetSupplyTypesQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplyTypeRequest request, CancellationToken ct)
    {
        try
        {
            var id = await mediator.Send(new CreateSupplyTypeCommand(
                request.Name, request.Unit, request.PricePerUnit,
                request.YieldBasis, request.YieldQuantity, request.YieldProductName), ct);
            return Ok(new { id });
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplyTypeRequest request, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new UpdateSupplyTypeCommand(
                id, request.Name, request.Unit, request.PricePerUnit,
                request.YieldBasis, request.YieldQuantity, request.YieldProductName), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new DeleteSupplyTypeCommand(id), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

public record CreateSupplyTypeRequest(
    string Name,
    string Unit,
    decimal? PricePerUnit,
    YieldBasis? YieldBasis = null,
    decimal? YieldQuantity = null,
    string? YieldProductName = null);

public record UpdateSupplyTypeRequest(
    string Name,
    string Unit,
    decimal? PricePerUnit,
    YieldBasis? YieldBasis = null,
    decimal? YieldQuantity = null,
    string? YieldProductName = null);
