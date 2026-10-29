using System.Net;
using System.Net.Http.Json;
using AuthentificationApi.Models;
using GestionOrdinateurs.Shared;

namespace AuthentificationApi.Services;

// demande un token à Keycloak avec le login / mot de passe de l'utilisateur (grant_type = password)
public class KeycloakService : IKeycloakService
{
    // nom du client HTTP déclaré dans Program.cs
    public const string NomClient = "Keycloak";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeycloakService> _logger;

    public KeycloakService(IHttpClientFactory httpClientFactory, IConfiguration configuration,
        ILogger<KeycloakService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponse?> ObtenirTokenAsync(string login, string motDePasse)
    {
        HttpClient client = _httpClientFactory.CreateClient(NomClient);
        string realm = _configuration["Keycloak:Realm"] ?? "";

        // Keycloak attend un formulaire (x-www-form-urlencoded) et non du JSON
        var formulaire = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _configuration["Keycloak:ClientId"] ?? "",
            ["client_secret"] = _configuration["Keycloak:ClientSecret"] ?? "",
            ["username"] = login,
            ["password"] = motDePasse
        });

        HttpResponseMessage reponse = await client.PostAsync($"realms/{realm}/protocol/openid-connect/token", formulaire);

        // Keycloak renvoie 400 "invalid_grant" (ou 401) quand le login ou le mot de passe est incorrect
        if (reponse.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
        {
            string detail = await reponse.Content.ReadAsStringAsync();
            _logger.LogWarning("Keycloak a refusé {Login} : {Detail}", login, detail);
            return null;
        }

        // toute autre erreur (Keycloak arrêté, 500...) déclenche une exception
        reponse.EnsureSuccessStatusCode();

        KeycloakTokenResponse? token = await reponse.Content.ReadFromJsonAsync<KeycloakTokenResponse>();
        if (token == null || string.IsNullOrWhiteSpace(token.AccessToken))
            return null;

        return new LoginResponse
        {
            Token = token.AccessToken,
            Expiration = DateTime.UtcNow.AddSeconds(token.ExpiresIn)
        };
    }
}
