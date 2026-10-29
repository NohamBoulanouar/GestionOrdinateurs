using FluentAssertions;
using GestionOrdinateurs.Shared;
using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.API.Services;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.Tests;

public class ReparationServiceTest
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

    [Fact]
    public async Task Test_CreerReparation_Devrait_Enregistrer_La_Reparation_En_Base()
    {
        ParcInformatiqueContext context = CreerContexte();
        var ordinateur = new Ordinateur
        {
            Id = 1,
            Nom = "Dell XPS",
            Caracteristiques = new Caracteristiques(16, 512, TypeOrdinateur.Portable, "Dell", "Intel i7")
        };
        context.Ordinateurs.Add(ordinateur);
        await context.SaveChangesAsync();

        var service = new ReparationService(context, new JournalService());

        Reparation reparation = await service.CreerReparationAsync(ordinateur);

        // la réparation est bien en base, liée à l'ordinateur, avec un technicien
        context.Reparations.Should().ContainSingle();
        reparation.OrdinateurId.Should().Be(1);
        reparation.Statut.Should().Be("En cours");
        reparation.Technicien.Should().NotBeNullOrEmpty();
    }
}
