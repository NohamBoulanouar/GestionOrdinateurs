using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionOrdinateurs.Shared;
using GestionOrdinateurs.API.Services;

namespace GestionOrdinateurs.API.Controllers;

[ApiController]
[Authorize] // token JWT obligatoire (Admin ou Gestionnaire)
[Route("api/[controller]")]
public class ReparationsController : ControllerBase
{
    private readonly ReparationService _reparService;
    private readonly IOrdinateurService _ordinateurService;
    private readonly StockService _stockService;
    private readonly ILogger<ReparationsController> _logger;

    public ReparationsController(ReparationService reparService, IOrdinateurService ordinateurService,
        StockService stockService, ILogger<ReparationsController> logger)
    {
        _reparService = reparService;
        _ordinateurService = ordinateurService;
        _stockService = stockService;
        _logger = logger;
    }

    // lance la réparation simultanée de tous les ordinateurs (Admin uniquement)
    [Authorize(Roles = "Admin")]
    [HttpPost("SimulerReparation")]
    public async Task<IActionResult> SimulerReparation()
    {
        List<Ordinateur> listOrdinateurs = await _ordinateurService.GetAllAsync();

        await Task.WhenAll(listOrdinateurs.Select(o => _reparService.SimulerReparationAsync(o)));

        return Ok($"{listOrdinateurs.Count} réparation(s) terminée(s)");
    }

    // liste des réparations avec les informations de l'ordinateur
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _reparService.GetAllAsync());
    }

    // historique des réparations (record HistoriqueReparationRecord)
    [HttpGet("historique")]
    public async Task<IActionResult> GetHistorique()
    {
        _logger.LogInformation("Récupération de l'historique des réparations.");
        return Ok(await _reparService.GetHistoriqueAsync());
    }

    // consomme 10 SSD en parallèle puis retourne le stock restant (Admin uniquement : modifie le stock)
    [Authorize(Roles = "Admin")]
    [HttpGet("GetStockSSD")]
    public IActionResult GetStockSSD()
    {
        Parallel.For(0, 10, i => _stockService.ConsommerSSD());

        _logger.LogInformation("StockSSD = {Stock}", _stockService.StockSSD);
        return Ok(_stockService.StockSSD);
    }
}
