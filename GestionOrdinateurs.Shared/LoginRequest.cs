namespace GestionOrdinateurs.Shared;

// corps de la requête POST de connexion envoyée à AuthentificationApi
public class LoginRequest
{
    public string Login { get; set; } = string.Empty;

    // mot de passe vérifié par Keycloak (TP6), ignoré par l'ancien endpoint login
    public string MotDePasse { get; set; } = string.Empty;
}
