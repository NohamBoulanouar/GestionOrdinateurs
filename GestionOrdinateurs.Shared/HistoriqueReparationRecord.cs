namespace GestionOrdinateurs.Shared;

// une ligne de l'historique des réparations
// (dans Shared pour être utilisée à la fois par l'API et par le client Blazor)
public record HistoriqueReparationRecord(
    int OrdinateurId,
    string NomOrdinateur,
    string Technicien,
    string TypeReparation,
    DateTime DateIntervention
);
