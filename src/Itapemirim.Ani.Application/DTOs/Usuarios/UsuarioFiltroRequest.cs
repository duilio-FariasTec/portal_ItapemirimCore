namespace Itapemirim.Ani.Application.DTOs.Usuarios;

public record UsuarioFiltroRequest(
    string? Busca = null,
    string? Status = null,
    Guid? PerfilGuid = null,
    int PageNumber = 1,
    int PageSize = 20
);