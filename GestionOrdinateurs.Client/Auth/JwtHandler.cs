using System.Net.Http.Headers;
using Blazored.SessionStorage;

namespace GestionOrdinateurs.Client.Auth;

// ajoute automatiquement le token JWT de la session dans l'en-tête Authorization de chaque requête
public class JwtHandler : DelegatingHandler
{
    private readonly ISessionStorageService _sessionStorage;

    public JwtHandler(ISessionStorageService sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? token = await _sessionStorage.GetItemAsync<string>(SessionKeys.JwtToken, cancellationToken);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(request, cancellationToken);
    }
}
