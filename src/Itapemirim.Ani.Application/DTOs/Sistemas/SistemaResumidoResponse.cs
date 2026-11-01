namespace Itapemirim.Ani.Application.DTOs.Sistemas;

public record SistemaResumidoResponse(
    Guid Id,
    string Codigo,
    string Nome,
    bool Ativo
);
