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
    private readonly IKeycloakService _keycloakService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(IUserApiService userApiService, ITokenService tokenService,
        IKeycloakService keycloakService, ILogger<AuthenticationController> logger)
    {
        _userApiService = userApiService;
        _tokenService = tokenService;
        _keycloakService = keycloakService;
        _logger = logger;
    }

    // TP5 : reçoit le login, vérifie que l'utilisateur existe puis renvoie un token JWT local (sans mot de passe)
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

    // TP6 : login + mot de passe vérifiés par Keycloak, le token renvoyé est signé par Keycloak
    [HttpPost("login-keycloak")]
    public async Task<IActionResult> LoginKeycloak([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.MotDePasse))
            return BadRequest("Le login et le mot de passe sont obligatoires");

        _logger.LogInformation("Tentative de connexion Keycloak de {Login}", request.Login);

        LoginResponse? reponse;
        try
        {
            reponse = await _keycloakService.ObtenirTokenAsync(request.Login, request.MotDePasse);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Keycloak est injoignable");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Keycloak est injoignable");
        }

        if (reponse == null)
        {
            _logger.LogWarning("Connexion Keycloak refusée pour {Login}", request.Login);
            return Unauthorized("Login ou mot de passe incorrect");
        }

        _logger.LogInformation("Token Keycloak obtenu pour {Login}", request.Login);
        return Ok(reponse);
    }
}
