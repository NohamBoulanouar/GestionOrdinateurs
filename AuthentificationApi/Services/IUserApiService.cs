using GestionOrdinateurs.Shared;

namespace AuthentificationApi.Services;

public interface IUserApiService
{
    Task<User?> GetByLoginAsync(string login);
}
