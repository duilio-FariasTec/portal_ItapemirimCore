namespace Itapemirim.Ani.Application.DTOs.Auth;

public record SistemaAcessoResponse(
    Guid SistemaId,
    string Codigo,
    string Nome,
    Guid PerfilId,
    string PerfilNome,
    List<string> Permissoes
);
