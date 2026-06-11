using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaTraction.Application.Sewing.Commands.CreateSewer;
using SistemaTraction.Application.Sewing.Commands.DeleteSewer;
using SistemaTraction.Application.Sewing.Commands.UpdateSewer;
using SistemaTraction.Application.Sewing.Queries.GetSewers;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.API.Controllers;

[Authorize]
[ApiController]
[Route("api/sewers")]
public class SewersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetSewersQuery(includeInactive), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSewerRequest request, CancellationToken ct)
    {
        try
        {
            var productTypes = request.ProductTypes
                .Select(pt => new ProductTypeInput(pt.Name, pt.PriceDefault, pt.PriceG1))
                .ToList();
            var result = await mediator.Send(new CreateSewerCommand(request.Name, request.Phone, productTypes), ct);
            return Created(string.Empty, result);
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSewerRequest request, CancellationToken ct)
    {
        try
        {
            var productTypes = request.ProductTypes
                .Select(pt => new ProductTypeInput(pt.Name, pt.PriceDefault, pt.PriceG1))
                .ToList();
            var result = await mediator.Send(new UpdateSewerCommand(id, request.Name, request.Phone, productTypes), ct);
            return Ok(result);
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new DeleteSewerCommand(id), ct);
            return NoContent();
        }
        catch (DomainException ex) { return NotFound(new { error = ex.Message }); }
    }
}

public record ProductTypeRequest(string Name, decimal PriceDefault, decimal PriceG1);
public record CreateSewerRequest(string Name, string? Phone, List<ProductTypeRequest> ProductTypes);
public record UpdateSewerRequest(string Name, string? Phone, List<ProductTypeRequest> ProductTypes);
