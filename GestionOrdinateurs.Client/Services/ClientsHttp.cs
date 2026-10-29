namespace GestionOrdinateurs.Client.Services;

// noms des clients HTTP déclarés dans Program.cs (IHttpClientFactory)
public static class ClientsHttp
{
    // AuthentificationApi, appelée sans token pour se connecter
    public const string Authentification = "AuthentificationApi";

    // Gateway YARP : toutes les autres API passent par elle, avec le token JWT
    public const string Gateway = "Gateway";
}
