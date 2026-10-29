using GestionOrdinateurs.Shared;
using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.API.Events;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Services;

public class OrdinateursService : IOrdinateurService
{
    private readonly ParcInformatiqueContext _context;
    private readonly JournalService _journal;

    // Définition des événements
    public event EventHandler<OrdinateurAjouteEventArgs>? OrdinateurAjoute;
    public event EventHandler<OrdinateurSupprimeEventArgs>? OrdinateurSupprime;
    public event EventHandler<OrdinateurMisAJourEventArgs>? OrdinateurMisAJour;

    public OrdinateursService(ParcInformatiqueContext context, JournalService journal)
    {
        _context = context;
        _journal = journal;
    }

    public async Task<List<Ordinateur>> GetAllAsync()
    {
        return await _context.Ordinateurs.ToListAsync();
    }

    public async Task<Ordinateur?> GetByIdAsync(int id)
    {
        return await _context.Ordinateurs.FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Ordinateur> AjouterOrdinateurAsync(Ordinateur ordinateur)
    {
        _context.Ordinateurs.Add(ordinateur);
        await _context.SaveChangesAsync();

        _journal.EcrireOrdinateur($"{ordinateur.Nom} ajouté");

        // Déclenche l'événement d'ajout 
        OrdinateurAjoute?.Invoke(this, new OrdinateurAjouteEventArgs { Ordinateur = ordinateur });

        return ordinateur;
    }

    public async Task<bool> MettreAJourOrdinateurAsync(int id, Ordinateur ordinateurModifie)
    {
        var ordinateurExistant = await _context.Ordinateurs.FirstOrDefaultAsync(o => o.Id == id);
        if (ordinateurExistant == null) return false;

        ordinateurExistant.Nom = ordinateurModifie.Nom;
        ordinateurExistant.Etat = ordinateurModifie.Etat;
        ordinateurExistant.Stock = ordinateurModifie.Stock;
        ordinateurExistant.Prix = ordinateurModifie.Prix;
        ordinateurExistant.Caracteristiques = ordinateurModifie.Caracteristiques;
        await _context.SaveChangesAsync();

        _journal.EcrireOrdinateur($"{ordinateurExistant.Nom} modifié");

        // Déclenche l'événement de mise à jour 
        OrdinateurMisAJour?.Invoke(this, new OrdinateurMisAJourEventArgs { Ordinateur = ordinateurExistant });

        return true;
    }

    public async Task<bool> SupprimerOrdinateurAsync(int id)
    {
        var ordinateur = await _context.Ordinateurs.FirstOrDefaultAsync(o => o.Id == id);
        if (ordinateur == null) return false;

        _context.Ordinateurs.Remove(ordinateur);
        await _context.SaveChangesAsync();

        _journal.EcrireOrdinateur($"{ordinateur.Nom} supprimé");

        // Déclenche l'événement de suppression
        OrdinateurSupprime?.Invoke(this, new OrdinateurSupprimeEventArgs { IdSupprime = id });

        return true;
    }

    public async Task<List<Ordinateur>> GetParMarqueAsync(string marque)
    {
        return await _context.Ordinateurs
            .Where(o => o.Caracteristiques.Marque.ToLower() == marque.ToLower())
            .ToListAsync();
    }

    // ordinateurs en stock (Stock > 0)
    public async Task<List<Ordinateur>> GetDisponiblesAsync()
    {
        return await _context.Ordinateurs
            .Where(o => o.Stock > 0)
            .ToListAsync();
    }

    // du plus cher au moins cher (tri en mémoire : SQLite ne sait pas trier un decimal)
    public async Task<List<Ordinateur>> GetParPrixDecroissantAsync()
    {
        List<Ordinateur> ordinateurs = await _context.Ordinateurs.ToListAsync();
        return ordinateurs.OrderByDescending(o => o.Prix).ToList();
    }

    // action conseillée selon l'état et les caractéristiques (pattern matching)
    public string Diagnostiquer(Ordinateur ordinateur)
    {
        return ordinateur switch
        {
            { Etat: EtatOrdinateur.HorsService } => "Remplacement",
            { Etat: EtatOrdinateur.Lent, Caracteristiques: { RamGo: < 8 } } => "Ajouter RAM",
            { Etat: EtatOrdinateur.EnPanne or EtatOrdinateur.Reparable } => "Réparation",
            _ => "Aucune action"
        };
    }

    // statistiques du parc sous forme de tuple nommé (calcul en mémoire à cause du decimal)
    public (int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations) CalculerStatistiques()
    {
        List<Ordinateur> ordinateurs = _context.Ordinateurs.ToList();

        return (
            ordinateurs.Count,
            ordinateurs.Count(o => o.Stock > 0),
            ordinateurs.Sum(o => o.Prix * o.Stock),
            _context.Reparations.Count()
        );
    }
}
