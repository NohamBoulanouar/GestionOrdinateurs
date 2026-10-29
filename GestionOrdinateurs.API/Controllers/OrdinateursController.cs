using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionOrdinateurs.Shared;
using GestionOrdinateurs.API.Services;

namespace GestionOrdinateurs.API.Controllers;

[ApiController]
[Authorize] // token JWT obligatoire (Admin ou Gestionnaire)
[Route("api/[controller]")]
public class OrdinateursController : ControllerBase
{
    private readonly IOrdinateurService _service;
    private readonly ILogger<OrdinateursController> _logger;
    private readonly IConfiguration _configuration;

    public OrdinateursController(IOrdinateurService service, ILogger<OrdinateursController> logger, IConfiguration configuration)
    {
        _service = service;
        _logger = logger;
        _configuration = configuration;

        //Abonnement aux événements pour afficher un message
        _service.OrdinateurAjoute += (sender, e) => 
            _logger.LogInformation($"[EVENT] L'ordinateur {e.Ordinateur.Nom} a été ajouté avec succès !");
            
        _service.OrdinateurSupprime += (sender, e) => 
            _logger.LogInformation($"[EVENT] L'ordinateur ID={e.IdSupprime} a été supprimé de la base.");
            
        _service.OrdinateurMisAJour += (sender, e) => 
            _logger.LogInformation($"[EVENT] L'ordinateur {e.Ordinateur.Nom} (ID={e.Ordinateur.Id}) a été mis à jour.");
    }

    // retourne tous les ordinateurs
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Récupération de la liste de tous les ordinateurs.");
        return Ok(await _service.GetAllAsync());
    }

    // retourne un ordinateur par son id, 404 si pas trouvé
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("Récupération de l'ordinateur avec l'ID: {Id}", id);
        Ordinateur? ordinateur = await _service.GetByIdAsync(id);
        if (ordinateur == null)
            return NotFound($"Aucun ordinateur avec l'ID {id}");

        return Ok(ordinateur);
    }

    // ajoute un ordinateur et retourne 201 (Admin uniquement)
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> AjouterOrdinateur([FromBody] Ordinateur ordinateur)
    {
        _logger.LogInformation("Tentative d'ajout d'un nouvel ordinateur.");
        if (ordinateur == null)
            return BadRequest("Données invalides");

        Ordinateur nouveau = await _service.AjouterOrdinateurAsync(ordinateur);
        return CreatedAtAction(nameof(GetById), new { id = nouveau.Id }, nouveau);
    }

    // met à jour un ordinateur existant (Admin uniquement)
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> MettreAJour(int id, [FromBody] Ordinateur ordinateurModifie)
    {
        _logger.LogInformation("Tentative de mise à jour de l'ordinateur avec l'ID: {Id}", id);
        if (ordinateurModifie == null)
            return BadRequest("Données invalides");

        bool succes = await _service.MettreAJourOrdinateurAsync(id, ordinateurModifie);
        if (!succes)
            return NotFound($"Aucun ordinateur avec l'ID {id}");

        return Ok(ordinateurModifie);
    }

    // supprime un ordinateur, 204 si ok (Admin uniquement)
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Supprimer(int id)
    {
        _logger.LogInformation("Tentative de suppression de l'ordinateur avec l'ID: {Id}", id);
        bool succes = await _service.SupprimerOrdinateurAsync(id);
        if (!succes)
            return NotFound($"Aucun ordinateur avec l'ID {id}");

        return NoContent();
    }

    // filtre les ordinateurs par marque
    [HttpGet("marque/{marque}")]
    public async Task<IActionResult> GetParMarque(string marque)
    {
        _logger.LogInformation("Recherche des ordinateurs pour la marque: {Marque}", marque);
        List<Ordinateur> resultats = await _service.GetParMarqueAsync(marque);
        if (resultats.Count == 0)
            return NotFound($"Aucun ordinateur de la marque '{marque}'");

        return Ok(resultats);
    }

    // retourne uniquement les ordinateurs disponibles (stock > 0)
    [HttpGet("disponibles")]
    public async Task<IActionResult> GetDisponibles()
    {
        _logger.LogInformation("Récupération des ordinateurs disponibles.");
        return Ok(await _service.GetDisponiblesAsync());
    }

    // retourne les ordinateurs du plus cher au moins cher
    [HttpGet("prix-decroissant")]
    public async Task<IActionResult> GetParPrixDecroissant()
    {
        _logger.LogInformation("Récupération des ordinateurs du plus cher au moins cher.");
        return Ok(await _service.GetParPrixDecroissantAsync());
    }

    // diagnostic d'un ordinateur selon son état et ses caractéristiques
    [HttpGet("{id}/diagnostic")]
    public async Task<IActionResult> GetDiagnostic(int id)
    {
        _logger.LogInformation("Diagnostic de l'ordinateur avec l'ID: {Id}", id);
        Ordinateur? ordinateur = await _service.GetByIdAsync(id);
        if (ordinateur == null)
            return NotFound($"Aucun ordinateur avec l'ID {id}");

        return Ok(new { ordinateur.Nom, ordinateur.Etat, Diagnostic = _service.Diagnostiquer(ordinateur) });
    }

    // statistiques du parc (déconstruction du tuple renvoyé par le service)
    [HttpGet("statistiques")]
    public IActionResult GetStatistiques()
    {
        _logger.LogInformation("Calcul des statistiques du parc.");
        var (total, enStock, valeurStock, nbReparations) = _service.CalculerStatistiques();

        return Ok(new { total, enStock, valeurStock, nbReparations });
    }

    // recherche dans l'inventaire par nom grâce à l'indexeur
    [HttpGet("inventaire/{nom}")]
    public async Task<IActionResult> Chercher(string nom)
    {
        _logger.LogInformation("Recherche dans l'inventaire de : {Nom}", nom);
        var inventaire = new InventaireOrdinateurs { Ordinateurs = await _service.GetAllAsync() };

        Ordinateur? ordinateur = inventaire[nom];
        if (ordinateur == null)
            return NotFound($"Aucun ordinateur nommé '{nom}'");

        return Ok(ordinateur);
    }

    // retourne les infos de l'application
    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        var appInfo = new 
        {
            Name = _configuration["Application:Name"],
            Version = _configuration["Application:Version"]
        };
        
        return Ok(appInfo);
    }
}