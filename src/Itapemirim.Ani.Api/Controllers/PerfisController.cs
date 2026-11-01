namespace Itapemirim.Ani.Api.Controllers;

using FluentValidation;
using Itapemirim.Ani.Application.DTOs.Perfis;
using Itapemirim.Ani.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/perfis")]
public class PerfisController : BaseApiController
{
    private readonly IPerfilService _service;

    public PerfisController(IPerfilService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool apenasAtivos = false, [FromQuery] Guid? sistemaGuid = null, CancellationToken ct = default)
    {
        var result = await _service.ListarAsync(apenasAtivos, sistemaGuid, ct);
        return Ok(result);
    }

    [HttpGet("{guid:guid}")]
    public async Task<IActionResult> ObterPorGuid(Guid guid, CancellationToken ct)
    {
        var result = await _service.ObterPorGuidAsync(guid, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(
        [FromBody] PerfilCreateRequest request,
        [FromServices] IValidator<PerfilCreateRequest> validator,
        CancellationToken ct)
    {
        var val = await validator.ValidateAsync(request, ct);
        if (!val.IsValid) throw new ValidationException(val.Errors);

        var result = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(ObterPorGuid), new { guid = result.Id }, result);
    }

    [HttpPut("{guid:guid}")]
    public async Task<IActionResult> Atualizar(Guid guid, [FromBody] PerfilUpdateRequest request, CancellationToken ct)
    {
        var result = await _service.AtualizarAsync(guid, request, ct);
        return Ok(result);
    }

    [HttpDelete("{guid:guid}")]
    public async Task<IActionResult> Inativar(Guid guid, CancellationToken ct)
    {
        await _service.InativarAsync(guid, ct);
        return NoContent();
    }

    [HttpGet("{guid:guid}/permissoes")]
    public async Task<IActionResult> ObterPermissoes(Guid guid, CancellationToken ct)
    {
        var result = await _service.ObterPermissoesAsync(guid, ct);
        return Ok(result);
    }

    [HttpPost("{guid:guid}/permissoes")]
    public async Task<IActionResult> AssociarPermissao(Guid guid, [FromBody] AssociarPermissaoRequest request, CancellationToken ct)
    {
        await _service.AssociarPermissaoAsync(guid, request.PermissaoId, ct);
        return NoContent();
    }

    [HttpDelete("{guid:guid}/permissoes/{permissaoGuid:guid}")]
    public async Task<IActionResult> RevogarPermissao(Guid guid, Guid permissaoGuid, CancellationToken ct)
    {
        await _service.RevogarPermissaoAsync(guid, permissaoGuid, ct);
        return NoContent();
    }
}