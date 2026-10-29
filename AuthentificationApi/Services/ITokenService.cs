using GestionOrdinateurs.Shared;

namespace AuthentificationApi.Services;

public interface ITokenService
{
    LoginResponse GenererToken(User user);
}
