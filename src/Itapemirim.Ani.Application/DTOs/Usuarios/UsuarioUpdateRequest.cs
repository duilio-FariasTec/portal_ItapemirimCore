namespace Itapemirim.Ani.Application.DTOs.Usuarios;

public record UsuarioUpdateRequest(
    string Nome,
    string? Email,
    string Status
);