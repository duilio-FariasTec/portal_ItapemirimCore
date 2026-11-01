namespace Itapemirim.Ani.Application.DTOs.Perfis;

public record PerfilResumidoResponse(
    Guid Id,
    string Nome,
    Guid SistemaId,
    string SistemaCodigo,
    string SistemaNome,
    DateTime ConcedidoEmUtc
);