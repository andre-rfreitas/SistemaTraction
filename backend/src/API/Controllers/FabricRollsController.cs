using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaTraction.Application.Fabric.Commands.RegisterFabricRoll;
using SistemaTraction.Application.Fabric.Queries.GetFabricRollById;
using SistemaTraction.Application.Fabric.Queries.GetFabricRolls;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.API.Controllers;

[Authorize]
[ApiController]
[Route("api/fabric-rolls")]
public class FabricRollsController(IMediator mediator) : ControllerBase
{
    // GET api/fabric-rolls?status=Available
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, CancellationToken ct)
    {
        var result = await mediator.Send(new GetFabricRollsQuery(status), ct);
        return Ok(result);
    }

    // GET api/fabric-rolls/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetFabricRollByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // POST api/fabric-rolls
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterFabricRollRequest request, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(
                new RegisterFabricRollCommand(request.FabricTypeId, request.FabricColorId, request.WeightKg, request.PriceTotal), ct);
            return CreatedAtAction(nameof(GetById), new { id = result.FabricRollId }, result);
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

public record RegisterFabricRollRequest(
    Guid FabricTypeId,
    Guid FabricColorId,
    decimal WeightKg,
    decimal PriceTotal
);
