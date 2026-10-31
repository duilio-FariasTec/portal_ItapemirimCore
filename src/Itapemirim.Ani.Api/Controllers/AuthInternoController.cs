namespace Itapemirim.Ani.Api.Controllers;

using Itapemirim.Ani.Application.DTOs.Auth;
using Itapemirim.Ani.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/auth")]
public class AuthInternoController : BaseApiController
{
    private readonly IAuthInternoService _service;

    public AuthInternoController(IAuthInternoService service) => _service = service;

    [HttpPost("validar-credenciais")]
    public async Task<IActionResult> Validar([FromBody] ValidarCredenciaisRequest request, CancellationToken ct)
    {
        var result = await _service.ValidarAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("login")]
    [HttpPost("/api/auth/login")]
    public async Task<IActionResult> Login([FromBody] ValidarCredenciaisRequest request, CancellationToken ct)
    {
        var result = await _service.LoginAsync(request, ct);
        return Ok(result);
    }
}