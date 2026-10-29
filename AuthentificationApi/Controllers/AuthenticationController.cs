using AuthentificationApi.Services;
using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace AuthentificationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IUserApiService _userApiService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(IUserApiService userApiService, ITokenService tokenService,
        ILogger<AuthenticationController> logger)
    {
        _userApiService = userApiService;
        _tokenService = tokenService;
        _logger = logger;
    }

    // reçoit le login, vérifie que l'utilisateur existe puis renvoie un token JWT (pas de mot de passe dans ce TP)
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login))
            return BadRequest("Le login est obligatoire");

        _logger.LogInformation("Tentative de connexion de {Login}", request.Login);

        User? user = await _userApiService.GetByLoginAsync(request.Login);
        if (user == null)
        {
            _logger.LogWarning("Connexion refusée : {Login} n'existe pas", request.Login);
            return NotFound($"L'utilisateur '{request.Login}' n'existe pas");
        }

        LoginResponse reponse = _tokenService.GenererToken(user);
        _logger.LogInformation("Token généré pour {Login} ({Role})", user.Login, user.Role);

        return Ok(reponse);
    }
}
