namespace Itapemirim.Ani.Application.DTOs.Perfis;

public record PerfilResponse(
    Guid Id,
    Guid SistemaId,
    string SistemaCodigo,
    string SistemaNome,
    string Nome,
    string? Descricao,
    bool Ativo,
    DateTime CriadoEmUtc,
    DateTime? AtualizadoEmUtc
);