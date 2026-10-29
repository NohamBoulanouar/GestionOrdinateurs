using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API.Services;

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByLoginAsync(string login);
}
