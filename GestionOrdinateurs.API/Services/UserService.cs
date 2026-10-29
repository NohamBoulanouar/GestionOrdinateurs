using GestionOrdinateurs.Shared;
using GestionOrdinateurs.API.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Services;

public class UserService : IUserService
{
    private readonly ParcInformatiqueContext _context;

    public UserService(ParcInformatiqueContext context)
    {
        _context = context;
    }

    // tous les utilisateurs
    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    // recherche par login, sans tenir compte de la casse (null si absent)
    public async Task<User?> GetByLoginAsync(string login)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Login.ToLower() == login.ToLower());
    }
}
