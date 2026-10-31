namespace Itapemirim.Ani.Application.DTOs.Usuarios;

public record UsuarioCreateRequest(
    string Nome,
    string Login,
    string? Email,
    string Senha,
    string? Status = "Ativo"
);