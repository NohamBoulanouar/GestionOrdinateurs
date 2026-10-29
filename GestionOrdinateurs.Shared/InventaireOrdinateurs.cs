namespace GestionOrdinateurs.Shared;

// collection spécialisée d'ordinateurs accessible par id ou par nom
public class InventaireOrdinateurs
{
    public List<Ordinateur> Ordinateurs { get; set; } = new();

    public int Count => Ordinateurs.Count;

    // recherche par id de l'ordinateur (null si absent)
    public Ordinateur? this[int ordinateurId] =>
        Ordinateurs.FirstOrDefault(o => o.Id == ordinateurId);

    // recherche par nom, sans tenir compte de la casse (null si absent)
    public Ordinateur? this[string nom] =>
        Ordinateurs.FirstOrDefault(o => o.Nom.Equals(nom, StringComparison.OrdinalIgnoreCase));
}
