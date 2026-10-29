using GestionOrdinateurs.Shared;

using GestionOrdinateurs.API.Events;

namespace GestionOrdinateurs.API.Services;

public interface IOrdinateurService
{
    // Événements
    event EventHandler<OrdinateurAjouteEventArgs> OrdinateurAjoute;
    event EventHandler<OrdinateurSupprimeEventArgs> OrdinateurSupprime;
    event EventHandler<OrdinateurMisAJourEventArgs> OrdinateurMisAJour;
    Task<List<Ordinateur>> GetAllAsync();
    Task<Ordinateur?> GetByIdAsync(int id);
    Task<Ordinateur> AjouterOrdinateurAsync(Ordinateur ordinateur);
    Task<bool> MettreAJourOrdinateurAsync(int id, Ordinateur ordinateurModifie);
    Task<bool> SupprimerOrdinateurAsync(int id);
    Task<List<Ordinateur>> GetParMarqueAsync(string marque);
    Task<List<Ordinateur>> GetDisponiblesAsync();
    Task<List<Ordinateur>> GetParPrixDecroissantAsync();
    string Diagnostiquer(Ordinateur ordinateur);
    (int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations) CalculerStatistiques();
}
