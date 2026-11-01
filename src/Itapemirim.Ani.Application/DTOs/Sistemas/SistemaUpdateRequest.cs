namespace Itapemirim.Ani.Application.DTOs.Sistemas;

public record SistemaUpdateRequest(
    string Codigo,
    string Nome,
    string? Descricao,
    bool Ativo
);
