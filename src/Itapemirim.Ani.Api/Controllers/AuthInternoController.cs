namespace Itapemirim.Ani.Api.Controllers;

using FluentValidation;
using Itapemirim.Ani.Application.DTOs.Auth;
using Itapemirim.Ani.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/auth")]
public class AuthInternoController : BaseApiController
{
    private readonly IAuthInternoService _service;

    public AuthInternoController(IAuthInternoService service) => _service = service;

    [HttpPost("validar-credenciais")]
    [ProducesResponseType(typeof(UsuarioAutenticadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Validar([FromBody] ValidarCredenciaisRequest request, CancellationToken ct)
    {
        var result = await _service.ValidarAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("login")]
    [HttpPost("/api/auth/login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Login([FromBody] ValidarCredenciaisRequest request, CancellationToken ct)
    {
        var result = await _service.LoginAsync(request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Solicita a redefinição de senha do usuário por login ou e-mail.
    /// Sempre retorna 200 OK independentemente da existência do usuário (proteção contra enumeração de contas).
    /// </summary>
    [HttpPost("reset-senha")]
    [HttpPost("forgot-password")]
    [HttpPost("/api/auth/reset-senha")]
    [HttpPost("/api/auth/forgot-password")]
    [ProducesResponseType(typeof(ResetSenhaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetSenha(
        [FromBody] ResetSenhaDto? request,
        [FromServices] IValidator<ResetSenhaDto> validator,
        CancellationToken ct)
    {
        if (request is null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("identificador", "O payload da requisição é obrigatório.")
            });
        }

        var validacao = await validator.ValidateAsync(request, ct);
        if (!validacao.IsValid)
        {
            throw new ValidationException(validacao.Errors);
        }

        var result = await _service.ResetarSenhaAsync(request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Realiza a troca de senha do usuário informando usuário (login ou e-mail), senha atual e nova senha.
    /// Limpa automaticamente a flag ExigeTrocaSenhaNoProximoLogin após a confirmação.
    /// </summary>
    [HttpPost("trocar-senha")]
    [HttpPost("alterar-senha")]
    [HttpPost("change-password")]
    [HttpPost("/api/auth/trocar-senha")]
    [HttpPost("/api/auth/alterar-senha")]
    [HttpPost("/api/auth/change-password")]
    [ProducesResponseType(typeof(TrocarSenhaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> TrocarSenha(
        [FromBody] TrocarSenhaDto? request,
        [FromServices] IValidator<TrocarSenhaDto> validator,
        CancellationToken ct)
    {
        if (request is null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("payload", "O payload da requisição é obrigatório.")
            });
        }

        var validacao = await validator.ValidateAsync(request, ct);
        if (!validacao.IsValid)
        {
            throw new ValidationException(validacao.Errors);
        }

        var result = await _service.TrocarSenhaAsync(request, ct);
        return Ok(result);
    }
}
