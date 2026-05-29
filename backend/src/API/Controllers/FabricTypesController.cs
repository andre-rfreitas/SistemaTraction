using MediatR;
using Microsoft.AspNetCore.Mvc;
using SistemaTraction.Application.Fabric.Commands.CreateFabricColor;
using SistemaTraction.Application.Fabric.Commands.CreateFabricType;
using SistemaTraction.Application.Fabric.Commands.DeleteFabricColor;
using SistemaTraction.Application.Fabric.Commands.DeleteFabricType;
using SistemaTraction.Application.Fabric.Commands.UpdateFabricColor;
using SistemaTraction.Application.Fabric.Commands.UpdateFabricType;
using SistemaTraction.Application.Fabric.Queries.GetFabricTypeById;
using SistemaTraction.Application.Fabric.Queries.GetFabricTypes;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.API.Controllers;

[ApiController]
[Route("api/fabric-types")]
public class FabricTypesController(IMediator mediator) : ControllerBase
{
    // GET api/fabric-types
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetFabricTypesQuery(), ct);
        return Ok(result);
    }

    // GET api/fabric-types/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetFabricTypeByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // POST api/fabric-types
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFabricTypeRequest request, CancellationToken ct)
    {
        try
        {
            var id = await mediator.Send(new CreateFabricTypeCommand(
                request.Name, request.Variation, request.PricePerKg,
                request.AverageKgPerRoll, request.AveragePiecesPerRoll), ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // PUT api/fabric-types/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFabricTypeRequest request, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new UpdateFabricTypeCommand(
                id, request.Name, request.Variation, request.PricePerKg,
                request.AverageKgPerRoll, request.AveragePiecesPerRoll), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // DELETE api/fabric-types/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new DeleteFabricTypeCommand(id), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // POST api/fabric-types/{id}/colors
    [HttpPost("{id:guid}/colors")]
    public async Task<IActionResult> AddColor(Guid id, [FromBody] CreateFabricColorRequest request, CancellationToken ct)
    {
        try
        {
            var colorId = await mediator.Send(new CreateFabricColorCommand(id, request.Name, request.HexCode), ct);
            return Created(string.Empty, new { id = colorId });
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // PUT api/fabric-types/{id}/colors/{colorId}
    [HttpPut("{id:guid}/colors/{colorId:guid}")]
    public async Task<IActionResult> UpdateColor(Guid id, Guid colorId, [FromBody] UpdateFabricColorRequest request, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new UpdateFabricColorCommand(colorId, request.Name, request.HexCode), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // DELETE api/fabric-types/{id}/colors/{colorId}
    [HttpDelete("{id:guid}/colors/{colorId:guid}")]
    public async Task<IActionResult> DeleteColor(Guid id, Guid colorId, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new DeleteFabricColorCommand(id, colorId), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

public record CreateFabricTypeRequest(string Name, string Variation, decimal PricePerKg, decimal AverageKgPerRoll, int? AveragePiecesPerRoll);
public record UpdateFabricTypeRequest(string Name, string Variation, decimal PricePerKg, decimal AverageKgPerRoll, int? AveragePiecesPerRoll);
public record CreateFabricColorRequest(string Name, string? HexCode);
public record UpdateFabricColorRequest(string Name, string? HexCode);
