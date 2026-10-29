using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace GestionOrdinateurs.Client.Auth;

// fournit à Blazor l'utilisateur connecté, reconstruit à partir du token JWT stocké en session
public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonyme = new(new ClaimsIdentity());

    private readonly ISessionStorageService _sessionStorage;

    public JwtAuthenticationStateProvider(ISessionStorageService sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token = await _sessionStorage.GetItemAsync<string>(SessionKeys.JwtToken);
        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(Anonyme);

        // token expiré (15 minutes) : on le supprime et l'utilisateur redevient anonyme
        if (EstExpire(token))
        {
            await _sessionStorage.RemoveItemAsync(SessionKeys.JwtToken);
            return new AuthenticationState(Anonyme);
        }

        return new AuthenticationState(CreerUtilisateur(token));
    }

    // appelé après la connexion pour mettre à jour l'interface (menu, pages protégées)
    public void NotifyUserAuthentication(string token)
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(CreerUtilisateur(token))));
    }

    // appelé après la déconnexion
    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonyme)));
    }

    private static ClaimsPrincipal CreerUtilisateur(string token)
    {
        var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), authenticationType: "jwt");
        return new ClaimsPrincipal(identity);
    }

    private static bool EstExpire(string token)
    {
        JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        return jwt.ValidTo < DateTime.UtcNow;
    }

    // dans le token les claims ont des noms courts ("role", "unique_name"...) :
    // on les convertit en ClaimTypes pour que [Authorize(Roles = ...)] et Identity.Name fonctionnent
    private static IEnumerable<Claim> ParseClaimsFromJwt(string token)
    {
        JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var claims = new List<Claim>();

        foreach (Claim claim in jwt.Claims)
        {
            string type = claim.Type switch
            {
                "role" => ClaimTypes.Role,
                "unique_name" => ClaimTypes.Name,
                "nameid" => ClaimTypes.NameIdentifier,
                _ => claim.Type
            };
            claims.Add(new Claim(type, claim.Value));
        }
        return claims;
    }
}
