namespace Itapemirim.Ani.Application.DTOs.Perfis;

public record PerfilUpdateRequest(
    Guid? SistemaId,
    string Nome,
    string? Descricao,
    bool Ativo
);