namespace Itapemirim.Ani.Application.DTOs.Perfis;

public record PerfilResponse(
    Guid Id,
    string Nome,
    string? Descricao,
    bool Ativo,
    DateTime CriadoEmUtc,
    DateTime? AtualizadoEmUtc
);