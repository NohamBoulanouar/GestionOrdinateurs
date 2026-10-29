namespace GestionOrdinateurs.Shared;

// États possibles d'un ordinateur
public enum EtatOrdinateur
{
    EnService,
    EnPanne,
    EnMaintenance,
    HorsService,
    Lent,
    Reparable
}
