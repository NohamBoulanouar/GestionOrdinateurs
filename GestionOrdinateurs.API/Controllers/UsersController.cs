using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionOrdinateurs.Shared;
using GestionOrdinateurs.API.Services;

namespace GestionOrdinateurs.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")] // seul un administrateur peut lister les utilisateurs
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // retourne tous les utilisateurs
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Récupération de la liste de tous les utilisateurs.");
        return Ok(await _userService.GetAllAsync());
    }

    // retourne un utilisateur par son login, 404 s'il n'existe pas
    // AllowAnonymous : appelé par AuthentificationApi AVANT la génération du token JWT
    [AllowAnonymous]
    [HttpGet("userbylogin/{login}")]
    public async Task<IActionResult> GetByLogin(string login)
    {
        _logger.LogInformation("Recherche de l'utilisateur avec le login: {Login}", login);
        User? user = await _userService.GetByLoginAsync(login);
        if (user == null)
            return NotFound($"L'utilisateur '{login}' n'existe pas");

        return Ok(user);
    }
}
