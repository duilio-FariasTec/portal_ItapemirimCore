namespace Itapemirim.Ani.Application.Interfaces;

using Itapemirim.Ani.Application.DTOs.Auth;

public interface IAuthInternoService
{
    Task<UsuarioAutenticadoResponse> ValidarAsync(ValidarCredenciaisRequest request, CancellationToken ct = default);
    Task<LoginResponse> LoginAsync(ValidarCredenciaisRequest request, CancellationToken ct = default);
}