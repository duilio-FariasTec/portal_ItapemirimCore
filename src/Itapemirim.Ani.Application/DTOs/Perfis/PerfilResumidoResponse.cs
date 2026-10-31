namespace Itapemirim.Ani.Application.DTOs.Perfis;

public record PerfilResumidoResponse(
    Guid Id,
    string Nome,
    DateTime ConcedidoEmUtc
);