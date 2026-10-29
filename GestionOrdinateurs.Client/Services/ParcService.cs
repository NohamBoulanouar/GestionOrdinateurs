using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.Client.Services;

// récupère les données du parc informatique en passant UNIQUEMENT par la Gateway
public class ParcService
{
    // l'API renvoie les enums en texte ("EnService") : il faut le convertisseur pour les relire
    private static readonly JsonSerializerOptions OptionsJson = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IHttpClientFactory _httpClientFactory;

    public ParcService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // GET Gateway -> api/Ordinateurs
    public async Task<List<Ordinateur>> GetOrdinateursAsync()
    {
        HttpClient client = _httpClientFactory.CreateClient(ClientsHttp.Gateway);
        return await client.GetFromJsonAsync<List<Ordinateur>>("Ordinateurs", OptionsJson) ?? [];
    }

    // GET Gateway -> api/Reparations/historique
    public async Task<List<HistoriqueReparationRecord>> GetHistoriqueReparationsAsync()
    {
        HttpClient client = _httpClientFactory.CreateClient(ClientsHttp.Gateway);
        return await client.GetFromJsonAsync<List<HistoriqueReparationRecord>>("Reparations/historique", OptionsJson) ?? [];
    }
}
