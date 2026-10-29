using System.Text.Json.Serialization;

namespace AuthentificationApi.Models;

// réponse du endpoint /protocol/openid-connect/token de Keycloak (seuls les champs utiles sont lus)
public class KeycloakTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    // durée de validité du token en secondes
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
