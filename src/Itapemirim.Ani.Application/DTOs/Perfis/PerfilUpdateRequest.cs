namespace Itapemirim.Ani.Application.DTOs.Perfis;

public record PerfilUpdateRequest(
    string Nome,
    string? Descricao,
    bool Ativo
);