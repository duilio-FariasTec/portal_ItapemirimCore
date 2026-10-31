namespace Itapemirim.Ani.Application.DTOs.Perfis;

public record PerfilCreateRequest(
    string Nome,
    string? Descricao,
    bool Ativo = true
);