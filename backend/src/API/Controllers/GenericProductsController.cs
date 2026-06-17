using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaTraction.Application.Stock.Commands.AdjustGenericProductStock;
using SistemaTraction.Application.Stock.Commands.CreateGenericProduct;
using SistemaTraction.Application.Stock.Commands.CreateGenericProductCategory;
using SistemaTraction.Application.Stock.Commands.DeleteGenericProduct;
using SistemaTraction.Application.Stock.Commands.DeleteGenericProductCategory;
using SistemaTraction.Application.Stock.Queries.GetGenericProductCategories;
using SistemaTraction.Application.Stock.Queries.GetGenericProductMovements;
using SistemaTraction.Application.Stock.Queries.GetGenericProducts;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.API.Controllers;

[Authorize]
[ApiController]
[Route("api/stock/generic-products")]
public class GenericProductsController(IMediator mediator) : ControllerBase
{
    // CATEGORIES

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var result = await mediator.Send(new GetGenericProductCategoriesQuery(), ct);
        return Ok(result);
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateGenericProductCategoryRequest request, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new CreateGenericProductCategoryCommand(request.Name), ct);
            return Ok(new { Id = result });
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpDelete("categories/{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new DeleteGenericProductCategoryCommand(id), ct);
            return Ok(new { Id = result });
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // PRODUCTS

    [HttpGet("categories/{categoryId:guid}/products")]
    public async Task<IActionResult> GetProducts(Guid categoryId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetGenericProductsQuery(categoryId), ct);
        return Ok(result);
    }

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateGenericProductRequest request, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(
                new CreateGenericProductCommand(
                    request.CategoryId,
                    request.Name,
                    request.InitialQuantity,
                    request.UnitCost,
                    request.Reason ?? "Cadastro inicial"), ct);
            return Ok(new { Id = result });
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpDelete("products/{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new DeleteGenericProductCommand(id), ct);
            return Ok(new { Id = result });
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // ADJUSTMENTS & MOVEMENTS

    [HttpPost("products/{id:guid}/adjustment")]
    public async Task<IActionResult> AdjustStock(Guid id, [FromBody] AdjustGenericProductStockRequest request, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(
                new AdjustGenericProductStockCommand(
                    id,
                    request.AdjustmentType,
                    request.Quantity,
                    request.Reason,
                    request.UnitCost), ct);
            return Ok(new { Id = result });
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet("products/{id:guid}/movements")]
    public async Task<IActionResult> GetMovements(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetGenericProductMovementsQuery(id, page, pageSize), ct);
        return Ok(result);
    }
}

public record CreateGenericProductCategoryRequest(string Name);

public record CreateGenericProductRequest(
    Guid CategoryId,
    string Name,
    int InitialQuantity,
    decimal UnitCost = 0m,
    string? Reason = null
);

public record AdjustGenericProductStockRequest(
    string AdjustmentType,
    int Quantity,
    string Reason,
    decimal UnitCost = 0m
);
