namespace Itapemirim.Ani.Application.DTOs.Usuarios;

using Itapemirim.Ani.Application.DTOs.Perfis;

public record UsuarioResponse(
    Guid Id,
    string Nome,
    string Login,
    string? Email,
    string Status,
    DateTime CriadoEmUtc,
    DateTime? AtualizadoEmUtc,
    List<PerfilResumidoResponse> PerfisAtivos
);