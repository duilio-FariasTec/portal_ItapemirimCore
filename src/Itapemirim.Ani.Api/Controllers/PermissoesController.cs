namespace Itapemirim.Ani.Api.Controllers;

using FluentValidation;
using Itapemirim.Ani.Application.DTOs.Permissoes;
using Itapemirim.Ani.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/permissoes")]
public class PermissoesController : BaseApiController
{
    private readonly IPermissaoService _service;

    public PermissoesController(IPermissaoService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool apenasAtivas = false, CancellationToken ct = default)
    {
        var result = await _service.ListarAsync(apenasAtivas, ct);
        return Ok(result);
    }

    [HttpGet("{guid:guid}")]
    public async Task<IActionResult> ObterPorGuid(Guid guid, CancellationToken ct)
    {
        var result = await _service.ObterPorGuidAsync(guid, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Cadastrar(
        [FromBody] PermissaoCreateRequest request,
        [FromServices] IValidator<PermissaoCreateRequest> validator,
        CancellationToken ct)
    {
        var val = await validator.ValidateAsync(request, ct);
        if (!val.IsValid) throw new ValidationException(val.Errors);

        var result = await _service.CadastrarAsync(request, ct);
        return CreatedAtAction(nameof(ObterPorGuid), new { guid = result.Id }, result);
    }

    [HttpPut("{guid:guid}")]
    public async Task<IActionResult> Atualizar(Guid guid, [FromBody] PermissaoUpdateRequest request, CancellationToken ct)
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
}