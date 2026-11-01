namespace Itapemirim.Ani.Application.DTOs.Auth;

public record PermissaoUsuarioAutenticadoResponse(
    Guid Id,
    string Codigo,
    string Nome,
    string? Descricao,
    string Recurso,
    string Acao,
    string? DscUrl
);
