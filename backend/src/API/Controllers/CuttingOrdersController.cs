using MediatR;
using Microsoft.AspNetCore.Mvc;
using SistemaTraction.Application.Cutting.Commands.CreateCuttingOrder;
using SistemaTraction.Application.Cutting.Commands.RegisterCuttingDelivery;
using SistemaTraction.Application.Cutting.Commands.SendCuttingOrder;
using SistemaTraction.Application.Cutting.Queries.GetCuttingOrderById;
using SistemaTraction.Application.Cutting.Queries.GetCuttingOrders;
using SistemaTraction.Application.Cutting.Queries.GetCuttingRecommendation;
using SistemaTraction.Application.Cutting.Queries.GetCuttingRecommendationHistory;
using SistemaTraction.Application.Sewing.Commands.RegisterSewingDelivery;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.API.Controllers;

[ApiController]
[Route("api/cutting-orders")]
public class CuttingOrdersController(IMediator mediator) : ControllerBase
{
    // GET api/cutting-orders/recommendation?fabricRollId={id}&daysBack={n}
    [HttpGet("recommendation")]
    public async Task<IActionResult> GetRecommendation(
        [FromQuery] Guid fabricRollId,
        [FromQuery] int? daysBack,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetCuttingRecommendationQuery(fabricRollId, daysBack), ct);
            return Ok(result);
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // GET api/cutting-orders/recommendation-history?take=10
    [HttpGet("recommendation-history")]
    public async Task<IActionResult> GetRecommendationHistory([FromQuery] int take = 10, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetCuttingRecommendationHistoryQuery(take), ct);
        return Ok(result);
    }

    // GET api/cutting-orders?status=Draft
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCuttingOrdersQuery(status), ct);
        return Ok(result);
    }

    // GET api/cutting-orders/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCuttingOrderByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // POST api/cutting-orders
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCuttingOrderRequest request, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(
                new CreateCuttingOrderCommand(
                    request.FabricRollId,
                    request.RequestedPieces,
                    request.Notes,
                    request.RecommendedPieces,
                    request.RecommendationDays,
                    request.RecommendationBasedOnOrders), ct);
            return CreatedAtAction(nameof(GetById), new { id = result.CuttingOrderId }, result);
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // POST api/cutting-orders/{id}/send
    [HttpPost("{id:guid}/send")]
    public async Task<IActionResult> Send(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new SendCuttingOrderCommand(id), ct);
            return Ok(result);
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // POST api/cutting-orders/{id}/delivery
    [HttpPost("{id:guid}/delivery")]
    public async Task<IActionResult> RegisterDelivery(Guid id, [FromBody] RegisterDeliveryRequest request, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new RegisterCuttingDeliveryCommand(id, request.DeliveredPieces), ct);
            return Ok(result);
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // POST api/cutting-orders/{id}/sewing-delivery
    [HttpPost("{id:guid}/sewing-delivery")]
    public async Task<IActionResult> RegisterSewingDelivery(Guid id, [FromBody] RegisterSewingDeliveryRequest request, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(
                new RegisterSewingDeliveryCommand(id, request.GoodPieces, request.DefectivePieces), ct);
            return Ok(result);
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

public record CreateCuttingOrderRequest(
    Guid FabricRollId,
    Dictionary<string, int> RequestedPieces,
    string? Notes,
    Dictionary<string, int>? RecommendedPieces = null,
    int? RecommendationDays = null,
    int? RecommendationBasedOnOrders = null
);

public record RegisterDeliveryRequest(Dictionary<string, int> DeliveredPieces);

public record RegisterSewingDeliveryRequest(
    Dictionary<string, int> GoodPieces,
    Dictionary<string, int> DefectivePieces
);
