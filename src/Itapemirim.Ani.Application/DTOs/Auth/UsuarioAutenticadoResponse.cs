namespace Itapemirim.Ani.Application.DTOs.Auth;

public record UsuarioAutenticadoResponse(
    Guid Id,
    string Nome,
    string Login,
    string? Email,
    string Status,
    List<string> Perfis,
    List<string> Permissoes
);