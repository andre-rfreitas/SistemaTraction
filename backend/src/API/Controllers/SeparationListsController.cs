using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaTraction.Application.Separation.Commands.ConfirmSeparationList;
using SistemaTraction.Application.Separation.Commands.DeleteSeparationList;
using SistemaTraction.Application.Separation.Commands.DeleteSkuCode;
using SistemaTraction.Application.Separation.Commands.RenameSeparationList;
using SistemaTraction.Application.Separation.Commands.UpdateSeparationItems;
using SistemaTraction.Application.Separation.Commands.UploadSeparationList;
using SistemaTraction.Application.Separation.Commands.UpsertSkuCode;
using SistemaTraction.Application.Separation.Queries.GetSeparationListById;
using SistemaTraction.Application.Separation.Queries.GetSeparationLists;
using SistemaTraction.Application.Separation.Queries.GetSkuCodes;
using SistemaTraction.Application.Separation.Queries.GetStockCheck;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.API.Controllers;

[Authorize]
[ApiController]
[Route("api/separation-lists")]
public class SeparationListsController(IMediator mediator) : ControllerBase
{
    // GET api/separation-lists
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetSeparationListsQuery(), ct);
        return Ok(result);
    }

    // GET api/separation-lists/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetSeparationListByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // GET api/separation-lists/{id}/stock-check
    [HttpGet("{id:guid}/stock-check")]
    public async Task<IActionResult> StockCheck(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetStockCheckQuery(id), ct);
            return Ok(result);
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // POST api/separation-lists/upload
    [HttpPost("upload")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "Nenhum arquivo enviado." });

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Apenas arquivos PDF são aceitos." });

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await mediator.Send(new UploadSeparationListCommand(stream, file.FileName), ct);
            return Ok(result);
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // PUT api/separation-lists/{id}/items
    [HttpPut("{id:guid}/items")]
    public async Task<IActionResult> UpdateItems(Guid id, [FromBody] UpdateItemsRequest request, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new UpdateSeparationItemsCommand(id, request.Items), ct);
            return Ok(result);
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // GET api/separation-lists/sku-codes
    [HttpGet("sku-codes")]
    public async Task<IActionResult> GetSkuCodes(CancellationToken ct)
        => Ok(await mediator.Send(new GetSkuCodesQuery(), ct));

    // POST api/separation-lists/sku-codes
    [HttpPost("sku-codes")]
    public async Task<IActionResult> UpsertSkuCode([FromBody] UpsertSkuCodeRequest request, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new UpsertSkuCodeCommand(request.Id, request.Code, request.Value, request.Category), ct);
            return Ok(result);
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // DELETE api/separation-lists/sku-codes/{id}
    [HttpDelete("sku-codes/{id:guid}")]
    public async Task<IActionResult> DeleteSkuCode(Guid id, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new DeleteSkuCodeCommand(id), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // DELETE api/separation-lists/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new DeleteSeparationListCommand(id), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // PATCH api/separation-lists/{id}/rename
    [HttpPatch("{id:guid}/rename")]
    public async Task<IActionResult> Rename(Guid id, [FromBody] RenameListRequest request, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new RenameSeparationListCommand(id, request.FileName), ct);
            return Ok(result);
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // POST api/separation-lists/{id}/confirm
    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new ConfirmSeparationListCommand(id), ct);
            return Ok(result);
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

public record UpdateItemsRequest(List<UpdateItemDto> Items);

public record UpsertSkuCodeRequest(Guid? Id, string Code, string Value, string Category);

public record RenameListRequest(string FileName);
