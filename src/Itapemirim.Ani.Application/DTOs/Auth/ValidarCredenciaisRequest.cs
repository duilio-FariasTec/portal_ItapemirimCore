namespace Itapemirim.Ani.Application.DTOs.Auth;

public record ValidarCredenciaisRequest(
    string Login,
    string Senha,
    string? SistemaCodigo = null
);