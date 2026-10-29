using GestionOrdinateurs.Shared;
using GestionOrdinateurs.API.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Services;

public class ReparationService
{
    // HistoriqueReparationRecord est maintenant dans GestionOrdinateurs.Shared (utilisé aussi par le client Blazor)

    private static readonly string[] Techniciens = ["Alice", "Bruno", "Chloé", "David"];

    private readonly ParcInformatiqueContext _context;
    private readonly JournalService _journal;

    // un DbContext n'est pas thread-safe : on sérialise ses accès pendant les réparations simultanées
    private readonly SemaphoreSlim _verrou = new(1, 1);

    public ReparationService(ParcInformatiqueContext context, JournalService journal)
    {
        _context = context;
        _journal = journal;
    }

    // simule une réparation de 2 secondes
    public async Task SimulerReparationAsync(Ordinateur ordinateur)
    {
        Reparation reparation = await CreerReparationAsync(ordinateur);

        await Task.Delay(2000);

        await EnregistrerAsync(() => reparation.Statut = "Terminée");
        _journal.EcrireReparation($"{ordinateur.Nom} réparé par {reparation.Technicien}");
        Console.WriteLine($"{ordinateur.Nom} réparé");
    }

    // crée une réparation en base, avec un technicien choisi au hasard
    public async Task<Reparation> CreerReparationAsync(Ordinateur ordinateur)
    {
        var reparation = new Reparation
        {
            OrdinateurId = ordinateur.Id,
            TypeReparation = "Diagnostic",
            Technicien = Techniciens[Random.Shared.Next(Techniciens.Length)],
            DateDebut = DateTime.Now,
            Statut = "En cours"
        };

        await EnregistrerAsync(() => _context.Reparations.Add(reparation));
        return reparation;
    }

    // liste des réparations avec l'ordinateur correspondant
    public async Task<List<Reparation>> GetAllAsync()
    {
        return await _context.Reparations
            .Include(r => r.UnOrdinateur)
            .ToListAsync();
    }

    // historique des réparations, de la plus récente à la plus ancienne
    public async Task<List<HistoriqueReparationRecord>> GetHistoriqueAsync()
    {
        return await _context.Reparations
            .OrderByDescending(r => r.DateDebut)
            .Select(r => new HistoriqueReparationRecord(
                r.OrdinateurId, r.UnOrdinateur.Nom, r.Technicien, r.TypeReparation, r.DateDebut))
            .ToListAsync();
    }

    private async Task EnregistrerAsync(Action modification)
    {
        await _verrou.WaitAsync();
        try
        {
            modification();
            await _context.SaveChangesAsync();
        }
        finally
        {
            _verrou.Release();
        }
    }
}
