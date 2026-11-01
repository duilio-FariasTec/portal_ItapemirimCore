namespace Itapemirim.Ani.Application.DTOs.Auth;

public record UsuarioAutenticadoResponse(
    Guid Id,
    string Nome,
    string Login,
    string? Email,
    string Status,
    List<SistemaAcessoResponse> Sistemas,
    List<PerfilUsuarioAutenticadoResponse> Perfis,
    List<PermissaoUsuarioAutenticadoResponse> Permissoes,
    List<string> Roles,
    List<string> PermissoesCodigos
);