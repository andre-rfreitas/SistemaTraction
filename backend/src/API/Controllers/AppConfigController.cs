using MediatR;
using Microsoft.AspNetCore.Mvc;
using SistemaTraction.Application.Config.Commands.UpsertAppConfig;
using SistemaTraction.Application.Config.Queries.GetAppConfigByKey;
using SistemaTraction.Application.Config.Queries.GetAppConfigs;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.API.Controllers;

[ApiController]
[Route("api/config")]
public class AppConfigController(IMediator mediator) : ControllerBase
{
    // GET api/config
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAppConfigsQuery(), ct);
        return Ok(result);
    }

    // GET api/config/{key}
    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(string key, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAppConfigByKeyQuery(key), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // PUT api/config/{key}
    [HttpPut("{key}")]
    public async Task<IActionResult> Upsert(
        string key, [FromBody] UpsertAppConfigRequest request, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new UpsertAppConfigCommand(key, request.Value), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

public record UpsertAppConfigRequest(string Value);
