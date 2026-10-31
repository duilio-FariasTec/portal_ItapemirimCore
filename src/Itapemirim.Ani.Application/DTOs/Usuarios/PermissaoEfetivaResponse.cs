namespace Itapemirim.Ani.Application.DTOs.Usuarios;

public record PermissaoEfetivaResponse(
    Guid Id,
    string Codigo,
    string Nome,
    string Recurso,
    string Acao
);