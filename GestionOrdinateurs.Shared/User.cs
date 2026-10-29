using System.ComponentModel.DataAnnotations;

namespace GestionOrdinateurs.Shared;

// Utilisateur de l'application (pas de mot de passe pour ce TP)
public class User
{
    // clé primaire
    public int Id { get; set; }

    // identifiant de connexion, unique en base (index configuré dans le DbContext)
    [Required]
    public string Login { get; set; } = string.Empty;

    public RoleEnum Role { get; set; }
}
