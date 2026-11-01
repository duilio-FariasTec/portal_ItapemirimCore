namespace Itapemirim.Ani.Application.DTOs.Sistemas;

public record SistemaResponse(
    Guid Id,
    string Codigo,
    string Nome,
    string? Descricao,
    bool Ativo,
    DateTime CriadoEmUtc,
    DateTime? AtualizadoEmUtc
);
