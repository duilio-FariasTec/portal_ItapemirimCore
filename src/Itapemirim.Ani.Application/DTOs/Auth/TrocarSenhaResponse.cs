namespace Itapemirim.Ani.Application.DTOs.Auth;

public record TrocarSenhaResponse(
    string Mensagem,
    DateTime AlteradoEmUtc
);

