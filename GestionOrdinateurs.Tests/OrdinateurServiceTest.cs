using FluentAssertions;
using GestionOrdinateurs.Shared;
using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.API.Services;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.Tests;

public class OrdinateurServiceTest
{
    // crée un contexte sur une base en mémoire, unique pour chaque test
    private ParcInformatiqueContext CreerContexte()
    {
        var options =
            new DbContextOptionsBuilder<ParcInformatiqueContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new ParcInformatiqueContext(options);
    }

    private OrdinateursService CreerService() => new OrdinateursService(CreerContexte(), new JournalService());

    private static Ordinateur NouvelOrdinateur(int id, string nom, int stock = 1, decimal prix = 0,
        EtatOrdinateur etat = EtatOrdinateur.EnService, int ramGo = 16)
    {
        return new Ordinateur
        {
            Id = id,
            Nom = nom,
            Etat = etat,
            Stock = stock,
            Prix = prix,
            Caracteristiques = new Caracteristiques(ramGo, 512, TypeOrdinateur.Portable, "Dell", "Intel i5")
        };
    }

    [Fact]
    public async Task Test_AjouterOrdinateur_Devrait_Ajouter_LOrdinateur_Dans_La_Liste()
    {
        OrdinateursService service = CreerService();
        int nombreAvant = (await service.GetAllAsync()).Count;

        await service.AjouterOrdinateurAsync(NouvelOrdinateur(99, "PC Test"));

        // vérifie que la liste a bien un élément de plus
        (await service.GetAllAsync()).Should().HaveCount(nombreAvant + 1);

        // vérifie que l'ordinateur ajouté est bien retrouvable par son id
        Ordinateur? trouve = await service.GetByIdAsync(99);
        trouve.Should().NotBeNull();
        trouve!.Nom.Should().Be("PC Test");
    }

    [Fact]
    public async Task Test_SupprimerOrdinateur_Existant_Devrait_Retourner_True()
    {
        OrdinateursService service = CreerService();

        // on ajoute un ordi pour être sûr qu'il existe avant de le supprimer
        await service.AjouterOrdinateurAsync(NouvelOrdinateur(77, "PC à supprimer"));

        bool resultat = await service.SupprimerOrdinateurAsync(77);

        // vérifie que la suppression a réussi et que l'ordi n'est plus dans la liste
        resultat.Should().BeTrue();
        (await service.GetByIdAsync(77)).Should().BeNull();
    }

    [Fact]
    public async Task Test_SupprimerOrdinateur_Inexistant_Devrait_Retourner_False()
    {
        OrdinateursService service = CreerService();

        // un id qui n'existe pas doit retourner false
        bool resultat = await service.SupprimerOrdinateurAsync(9999);
        resultat.Should().BeFalse();
    }

    [Fact]
    public async Task Test_GetDisponibles_Devrait_Retourner_Seulement_Les_Ordinateurs_En_Stock()
    {
        OrdinateursService service = CreerService();
        await service.AjouterOrdinateurAsync(NouvelOrdinateur(1, "En stock", stock: 5));
        await service.AjouterOrdinateurAsync(NouvelOrdinateur(2, "Rupture", stock: 0));

        List<Ordinateur> disponibles = await service.GetDisponiblesAsync();

        disponibles.Should().ContainSingle().Which.Nom.Should().Be("En stock");
    }

    [Fact]
    public async Task Test_GetParPrixDecroissant_Devrait_Trier_Du_Plus_Cher_Au_Moins_Cher()
    {
        OrdinateursService service = CreerService();
        await service.AjouterOrdinateurAsync(NouvelOrdinateur(1, "Moyen", prix: 500));
        await service.AjouterOrdinateurAsync(NouvelOrdinateur(2, "Cher", prix: 1500));
        await service.AjouterOrdinateurAsync(NouvelOrdinateur(3, "Pas cher", prix: 200));

        List<Ordinateur> tries = await service.GetParPrixDecroissantAsync();

        tries.Select(o => o.Nom).Should().ContainInOrder("Cher", "Moyen", "Pas cher");
    }

    [Fact]
    public void Test_Diagnostiquer_Ordinateur_Lent_Avec_Peu_De_Ram_Devrait_Conseiller_Ajouter_Ram()
    {
        OrdinateursService service = CreerService();

        service.Diagnostiquer(NouvelOrdinateur(1, "Vieux PC", etat: EtatOrdinateur.Lent, ramGo: 4))
            .Should().Be("Ajouter RAM");
        service.Diagnostiquer(NouvelOrdinateur(2, "PC HS", etat: EtatOrdinateur.HorsService))
            .Should().Be("Remplacement");
    }

    [Fact]
    public async Task Test_CalculerStatistiques_Devrait_Compter_Le_Parc()
    {
        OrdinateursService service = CreerService();
        await service.AjouterOrdinateurAsync(NouvelOrdinateur(1, "A", stock: 2, prix: 100));
        await service.AjouterOrdinateurAsync(NouvelOrdinateur(2, "B", stock: 0, prix: 500));

        var (total, disponibles, valeurStock, nbReparations) = service.CalculerStatistiques();

        total.Should().Be(2);
        disponibles.Should().Be(1);
        valeurStock.Should().Be(200);
        nbReparations.Should().Be(0);
    }
}
