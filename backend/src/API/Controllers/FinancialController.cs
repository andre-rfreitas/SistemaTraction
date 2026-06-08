using MediatR;
using Microsoft.AspNetCore.Mvc;
using SistemaTraction.Application.Financial.Commands.CreateFinancialEntry;
using SistemaTraction.Application.Financial.Commands.ReverseFinancialEntry;
using SistemaTraction.Application.Financial.Commands.SyncShopifyOrders;
using SistemaTraction.Application.Financial.Queries.GetFinancialEntries;
using SistemaTraction.Application.Financial.Queries.GetFinancialSummary;
using SistemaTraction.Application.Financial.Queries.GetShopifyStatus;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.API.Controllers;

[ApiController]
[Route("api/financial")]
public class FinancialController(IMediator mediator) : ControllerBase
{
    // GET api/financial/summary?from=...&to=...
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var result = await mediator.Send(new GetFinancialSummaryQuery(from, to), ct);
        return Ok(result);
    }

    // GET api/financial/entries?category=...&from=...&to=...
    [HttpGet("entries")]
    public async Task<IActionResult> GetEntries(
        [FromQuery] string? category,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetFinancialEntriesQuery(category, from, to), ct);
        return Ok(result);
    }

    // POST api/financial/entries
    [HttpPost("entries")]
    public async Task<IActionResult> Create([FromBody] CreateFinancialEntryRequest request, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(
                new CreateFinancialEntryCommand(request.Type, request.Category, request.Amount, request.Description), ct);
            return Ok(result);
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // POST api/financial/entries/{id}/reverse
    [HttpPost("entries/{id:guid}/reverse")]
    public async Task<IActionResult> Reverse(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new ReverseFinancialEntryCommand(id), ct);
            return Ok(result);
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // POST api/financial/shopify/sync
    [HttpPost("shopify/sync")]
    public async Task<IActionResult> ShopifySync([FromBody] ShopifySyncRequest request, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new SyncShopifyOrdersCommand(request.From, request.To), ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // GET api/financial/shopify/status
    [HttpGet("shopify/status")]
    public async Task<IActionResult> ShopifyStatus(CancellationToken ct)
    {
        var result = await mediator.Send(new GetShopifyStatusQuery(), ct);
        return Ok(result);
    }
}

public record CreateFinancialEntryRequest(
    string Type,
    string Category,
    decimal Amount,
    string Description
);

public record ShopifySyncRequest(DateOnly From, DateOnly To);
