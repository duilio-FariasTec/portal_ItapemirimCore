namespace Itapemirim.Ani.Application.DTOs.Perfis;

public record PerfilCreateRequest(
    Guid SistemaId,
    string Nome,
    string? Descricao,
    bool Ativo = true
);