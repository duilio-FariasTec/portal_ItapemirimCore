namespace Itapemirim.Ani.Application.DTOs.Auth;

public record PerfilUsuarioAutenticadoResponse(
    Guid Id,
    string Nome,
    string? Descricao,
    List<PermissaoUsuarioAutenticadoResponse> Permissoes
);
