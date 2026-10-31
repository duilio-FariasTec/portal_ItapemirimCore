namespace Itapemirim.Ani.Application.DTOs.Permissoes;

public record PermissaoUpdateRequest(
    string Nome,
    string? Descricao,
    string Recurso,
    string Acao,
    bool Ativo
);