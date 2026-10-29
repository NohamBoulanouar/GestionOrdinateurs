using System.Net;
using System.Net.Http.Json;
using Blazored.SessionStorage;
using GestionOrdinateurs.Client.Auth;
using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.Client.Services;

// connexion / déconnexion de l'utilisateur
public class AuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISessionStorageService _sessionStorage;
    private readonly JwtAuthenticationStateProvider _authProvider;

    public AuthService(IHttpClientFactory httpClientFactory, ISessionStorageService sessionStorage,
        JwtAuthenticationStateProvider authProvider)
    {
        _httpClientFactory = httpClientFactory;
        _sessionStorage = sessionStorage;
        _authProvider = authProvider;
    }

    // demande un token Keycloak à AuthentificationApi et le sauvegarde en session
    // retourne null si tout s'est bien passé, sinon le message d'erreur à afficher
    public async Task<string?> ConnecterAsync(string login, string motDePasse)
    {
        HttpClient client = _httpClientFactory.CreateClient(ClientsHttp.Authentification);

        HttpResponseMessage reponse;
        try
        {
            reponse = await client.PostAsJsonAsync("Authentication/login-keycloak",
                new LoginRequest { Login = login, MotDePasse = motDePasse });
        }
        catch (HttpRequestException)
        {
            return "Impossible de joindre le service d'authentification.";
        }

        if (reponse.StatusCode == HttpStatusCode.Unauthorized)
            return "Login ou mot de passe incorrect.";

        if (reponse.StatusCode == HttpStatusCode.ServiceUnavailable)
            return "Keycloak est injoignable.";

        if (!reponse.IsSuccessStatusCode)
            return $"Erreur lors de la connexion ({(int)reponse.StatusCode}).";

        LoginResponse? loginResponse = await reponse.Content.ReadFromJsonAsync<LoginResponse>();
        if (loginResponse == null || string.IsNullOrWhiteSpace(loginResponse.Token))
            return "Le token reçu est vide.";

        await _sessionStorage.SetItemAsync(SessionKeys.JwtToken, loginResponse.Token);
        _authProvider.NotifyUserAuthentication(loginResponse.Token);
        return null;
    }

    // supprime le token de la session
    public async Task DeconnecterAsync()
    {
        await _sessionStorage.RemoveItemAsync(SessionKeys.JwtToken);
        _authProvider.NotifyUserLogout();
    }
}
