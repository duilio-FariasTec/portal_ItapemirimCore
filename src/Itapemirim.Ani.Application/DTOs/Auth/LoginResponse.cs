namespace Itapemirim.Ani.Application.DTOs.Auth;

public record LoginResponse(
    string Token,
    string TokenType,
    int ExpiresInSeconds,
    UsuarioAutenticadoResponse Usuario
);
