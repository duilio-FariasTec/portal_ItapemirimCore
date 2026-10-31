namespace Itapemirim.Ani.Application.DTOs.Permissoes;

public record PermissaoResponse(
    Guid Id,
    string Codigo,
    string Nome,
    string? Descricao,
    string Recurso,
    string Acao,
    string? DscUrl,
    bool Ativo,
    DateTime CriadoEmUtc,
    DateTime? AtualizadoEmUtc
);