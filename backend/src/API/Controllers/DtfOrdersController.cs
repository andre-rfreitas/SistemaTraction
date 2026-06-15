using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaTraction.Application.Dtf.Commands.CancelDtfOrder;
using SistemaTraction.Application.Dtf.Commands.CreateDtfOrder;
using SistemaTraction.Application.Dtf.Commands.ReceiveDtfOrder;
using SistemaTraction.Application.Dtf.Commands.SendDtfOrder;
using SistemaTraction.Application.Dtf.Commands.UpdateDtfOrder;
using SistemaTraction.Application.Dtf.Queries.GetDtfOrders;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.API.Controllers;

[Authorize]
[ApiController]
[Route("api/dtf-orders")]
public class DtfOrdersController(IMediator mediator) : ControllerBase
{
    // GET api/dtf-orders?status=Draft
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, CancellationToken ct)
    {
        var result = await mediator.Send(new GetDtfOrdersQuery(status), ct);
        return Ok(result);
    }

    // POST api/dtf-orders
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDtfOrderRequest request, CancellationToken ct)
    {
        try
        {
            var items = request.Items
                .Select(i => new CreateDtfOrderItemInput(i.DtfModelId, i.SheetQuantity))
                .ToList();
            var id = await mediator.Send(new CreateDtfOrderCommand(request.Notes, items), ct);
            return Ok(new { id });
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // PUT api/dtf-orders/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDtfOrderRequest request, CancellationToken ct)
    {
        try
        {
            var items = request.Items
                .Select(i => new UpdateDtfOrderItemInput(i.DtfModelId, i.SheetQuantity))
                .ToList();
            await mediator.Send(new UpdateDtfOrderCommand(id, request.Notes, items), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // POST api/dtf-orders/{id}/send
    [HttpPost("{id:guid}/send")]
    public async Task<IActionResult> Send(Guid id, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new SendDtfOrderCommand(id), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // POST api/dtf-orders/{id}/receive
    [HttpPost("{id:guid}/receive")]
    public async Task<IActionResult> Receive(Guid id, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new ReceiveDtfOrderCommand(id), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // DELETE api/dtf-orders/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new CancelDtfOrderCommand(id), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

public record CreateDtfOrderItemRequest(Guid DtfModelId, int SheetQuantity);
public record CreateDtfOrderRequest(string? Notes, List<CreateDtfOrderItemRequest> Items);

public record UpdateDtfOrderItemRequest(Guid DtfModelId, int SheetQuantity);
public record UpdateDtfOrderRequest(string? Notes, List<UpdateDtfOrderItemRequest> Items);
