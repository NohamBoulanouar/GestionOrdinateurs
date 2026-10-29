using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GestionOrdinateurs.Shared;

namespace AuthentificationApi.Services;

// récupère les utilisateurs auprès de GestionOrdinateurs.API grâce à IHttpClientFactory
public class UserApiService : IUserApiService
{
    // nom du client HTTP déclaré dans Program.cs
    public const string NomClient = "GestionOrdinateursApi";

    // l'API renvoie les enums en texte ("Admin") : on doit pouvoir les relire
    private static readonly JsonSerializerOptions OptionsJson = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IHttpClientFactory _httpClientFactory;

    public UserApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // null si l'utilisateur n'existe pas (404 renvoyé par l'API)
    public async Task<User?> GetByLoginAsync(string login)
    {
        HttpClient client = _httpClientFactory.CreateClient(NomClient);

        HttpResponseMessage reponse = await client.GetAsync($"Users/userbylogin/{Uri.EscapeDataString(login)}");
        if (reponse.StatusCode == HttpStatusCode.NotFound)
            return null;

        // toute autre erreur (API arrêtée, 500...) déclenche une exception
        reponse.EnsureSuccessStatusCode();
        return await reponse.Content.ReadFromJsonAsync<User>(OptionsJson);
    }
}
