namespace Itapemirim.Ani.Application.DTOs.Permissoes;

public record PermissaoCreateRequest(
    string Codigo,
    string Nome,
    string? Descricao,
    string Recurso,
    string Acao,
    bool Ativo = true
);