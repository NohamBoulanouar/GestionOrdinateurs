using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionOrdinateurs.Shared;

[Table("Ordinateur")]
public class Ordinateur
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public EtatOrdinateur Etat { get; set; }

    [DefaultValue(0)]
    public int Stock { get; set; }

    [DefaultValue(0)]
    public decimal Prix { get; set; }

    // true quand l'ordinateur a été vendu (false par défaut)
    [DefaultValue(false)]
    public bool Vendu { get; set; } = false;

    public Caracteristiques Caracteristiques { get; set; } = new();

    public List<Reparation> Reparations { get; set; } = new();
}
