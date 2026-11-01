namespace Itapemirim.Ani.Application.DTOs.Auth;

public record ResetSenhaResponse(
    string Mensagem,
    DateTime ProcessadoEmUtc
);

