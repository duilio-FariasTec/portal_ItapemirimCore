namespace Itapemirim.Ani.Application.DTOs.Auth;

using System.Text.Json.Serialization;

public record ResetSenhaDto(
    [property: JsonPropertyName("identificador")] string? Identificador = null,
    [property: JsonPropertyName("login")] string? Login = null,
    [property: JsonPropertyName("email")] string? Email = null
)
{
    [JsonIgnore]
    public string ValorIdentificador =>
        !string.IsNullOrWhiteSpace(Identificador) ? Identificador.Trim() :
        !string.IsNullOrWhiteSpace(Login) ? Login.Trim() :
        !string.IsNullOrWhiteSpace(Email) ? Email.Trim() : string.Empty;
}

