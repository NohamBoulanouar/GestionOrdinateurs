using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestionOrdinateurs.Shared;
using Microsoft.IdentityModel.Tokens;

namespace AuthentificationApi.Services;

// génère les tokens JWT signés avec la clé Jwt:Key de appsettings.json
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public LoginResponse GenererToken(User user)
    {
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "");
        int dureeMinutes = _configuration.GetValue("Jwt:DureeMinutes", 15);
        DateTime expiration = DateTime.UtcNow.AddMinutes(dureeMinutes);

        // claims : nom de l'utilisateur, son rôle (utilisé par [Authorize(Roles = ...)]) et son id
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration, // validité de 15 minutes
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

        return new LoginResponse
        {
            Token = tokenHandler.WriteToken(token),
            Expiration = expiration
        };
    }
}
