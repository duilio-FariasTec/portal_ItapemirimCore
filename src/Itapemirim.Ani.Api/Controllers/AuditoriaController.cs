namespace Itapemirim.Ani.Api.Controllers;

using Itapemirim.Ani.Application.DTOs.Auditoria;
using Itapemirim.Ani.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/auditoria")]
public class AuditoriaController : BaseApiController
{
    private readonly IAuditoriaService _service;

    public AuditoriaController(IAuditoriaService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] AuditoriaFiltroRequest request, CancellationToken ct)
    {
        var result = await _service.ListarAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{guid:guid}")]
    public async Task<IActionResult> ObterPorGuid(Guid guid, CancellationToken ct)
    {
        var result = await _service.ObterPorGuidAsync(guid, ct);
        if (result == null) return NotFound();
        return Ok(result);
    }
}