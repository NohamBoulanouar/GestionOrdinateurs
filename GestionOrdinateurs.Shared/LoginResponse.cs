namespace GestionOrdinateurs.Shared;

// réponse d'AuthentificationApi : le token JWT généré pour l'utilisateur
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    // date d'expiration du token (UTC)
    public DateTime Expiration { get; set; }
}
