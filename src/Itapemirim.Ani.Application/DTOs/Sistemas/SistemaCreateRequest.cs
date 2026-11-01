namespace Itapemirim.Ani.Application.DTOs.Sistemas;

public record SistemaCreateRequest(
    string Codigo,
    string Nome,
    string? Descricao,
    bool Ativo = true
);
