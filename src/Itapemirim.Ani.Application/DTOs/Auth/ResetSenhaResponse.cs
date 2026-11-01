namespace Itapemirim.Ani.Application.DTOs.Auth;

using System.Text.Json.Serialization;

public record ResetSenhaResponse(
    string Mensagem,
    DateTime ProcessadoEmUtc,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? SenhaTemporaria = null
);
