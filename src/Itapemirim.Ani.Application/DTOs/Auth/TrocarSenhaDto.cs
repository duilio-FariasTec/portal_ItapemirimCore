namespace Itapemirim.Ani.Application.DTOs.Auth;

using System.Text.Json.Serialization;

public record TrocarSenhaDto(
    [property: JsonPropertyName("usuario")] string? Usuario = null,
    [property: JsonPropertyName("senhaAtual")] string SenhaAtual = "",
    [property: JsonPropertyName("novaSenha")] string NovaSenha = "",
    [property: JsonPropertyName("login")] string? Login = null,
    [property: JsonPropertyName("email")] string? Email = null
)
{
    [JsonIgnore]
    public string ValorUsuario =>
        !string.IsNullOrWhiteSpace(Usuario) ? Usuario.Trim() :
        !string.IsNullOrWhiteSpace(Login) ? Login.Trim() :
        !string.IsNullOrWhiteSpace(Email) ? Email.Trim() : string.Empty;
}

