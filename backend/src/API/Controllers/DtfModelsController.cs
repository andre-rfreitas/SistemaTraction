using MediatR;
using Microsoft.AspNetCore.Mvc;
using SistemaTraction.Application.Dtf.Commands.CreateDtfModel;
using SistemaTraction.Application.Dtf.Commands.DeleteDtfModel;
using SistemaTraction.Application.Dtf.Commands.UpdateDtfModel;
using SistemaTraction.Application.Dtf.Queries.GetDtfModelById;
using SistemaTraction.Application.Dtf.Queries.GetDtfModels;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.API.Controllers;

[ApiController]
[Route("api/dtf-models")]
public class DtfModelsController(IMediator mediator) : ControllerBase
{
    // GET api/dtf-models
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetDtfModelsQuery(), ct);
        return Ok(result);
    }

    // GET api/dtf-models/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetDtfModelByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // POST api/dtf-models
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDtfModelRequest request, CancellationToken ct)
    {
        try
        {
            var id = await mediator.Send(new CreateDtfModelCommand(
                request.Name, request.SheetLabel, request.StampsPerSheet, request.SheetCost), ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // PUT api/dtf-models/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDtfModelRequest request, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new UpdateDtfModelCommand(
                id, request.Name, request.SheetLabel, request.StampsPerSheet, request.SheetCost), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // DELETE api/dtf-models/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new DeleteDtfModelCommand(id), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

public record CreateDtfModelRequest(string Name, string SheetLabel, int StampsPerSheet, decimal SheetCost);
public record UpdateDtfModelRequest(string Name, string SheetLabel, int StampsPerSheet, decimal SheetCost);
