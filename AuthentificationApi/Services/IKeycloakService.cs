using GestionOrdinateurs.Shared;

namespace AuthentificationApi.Services;

public interface IKeycloakService
{
    // null si le login ou le mot de passe est refusé par Keycloak
    Task<LoginResponse?> ObtenirTokenAsync(string login, string motDePasse);
}
