namespace Itapemirim.Ani.Application.DTOs.Auth;

public record PerfilUsuarioAutenticadoResponse(
    Guid Id,
    string Nome,
    string? Descricao,
    Guid SistemaId,
    string SistemaCodigo,
    string SistemaNome,
    List<PermissaoUsuarioAutenticadoResponse> Permissoes
);
