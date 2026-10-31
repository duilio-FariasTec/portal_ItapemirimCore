namespace Itapemirim.Ani.Api.Controllers;

using FluentValidation;
using Itapemirim.Ani.Application.DTOs.Usuarios;
using Itapemirim.Ani.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/usuarios")]
public class UsuariosController : BaseApiController
{
    private readonly IUsuarioService _service;

    public UsuariosController(IUsuarioService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] UsuarioFiltroRequest request, CancellationToken ct)
    {
        var result = await _service.ListarAsync(request, ct);
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
        [FromBody] UsuarioCreateRequest request,
        [FromServices] IValidator<UsuarioCreateRequest> validator,
        CancellationToken ct)
    {
        var val = await validator.ValidateAsync(request, ct);
        if (!val.IsValid) throw new ValidationException(val.Errors);

        var result = await _service.CadastrarAsync(request, ct);
        return CreatedAtAction(nameof(ObterPorGuid), new { guid = result.Id }, result);
    }

    [HttpPut("{guid:guid}")]
    public async Task<IActionResult> Atualizar(
        Guid guid,
        [FromBody] UsuarioUpdateRequest request,
        [FromServices] IValidator<UsuarioUpdateRequest> validator,
        CancellationToken ct)
    {
        var val = await validator.ValidateAsync(request, ct);
        if (!val.IsValid) throw new ValidationException(val.Errors);

        var result = await _service.AtualizarAsync(guid, request, ct);
        return Ok(result);
    }

    [HttpDelete("{guid:guid}")]
    public async Task<IActionResult> Inativar(Guid guid, CancellationToken ct)
    {
        await _service.InativarAsync(guid, ct);
        return NoContent();
    }

    [HttpGet("{guid:guid}/perfis")]
    public async Task<IActionResult> ObterPerfis(Guid guid, CancellationToken ct)
    {
        var result = await _service.ObterPerfisAsync(guid, ct);
        return Ok(result);
    }

    [HttpPost("{guid:guid}/perfis")]
    public async Task<IActionResult> AtribuirPerfil(Guid guid, [FromBody] AtribuirPerfilRequest request, CancellationToken ct)
    {
        await _service.AtribuirPerfilAsync(guid, request.PerfilId, ct);
        return NoContent();
    }

    [HttpDelete("{guid:guid}/perfis/{perfilGuid:guid}")]
    public async Task<IActionResult> RevogarPerfil(Guid guid, Guid perfilGuid, CancellationToken ct)
    {
        await _service.RevogarPerfilAsync(guid, perfilGuid, ct);
        return NoContent();
    }

    [HttpGet("{guid:guid}/permissoes-efetivas")]
    public async Task<IActionResult> ObterPermissoesEfetivas(Guid guid, CancellationToken ct)
    {
        var result = await _service.ObterPermissoesEfetivasAsync(guid, ct);
        return Ok(result);
    }
}