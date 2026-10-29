using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API.Events;

public class OrdinateurAjouteEventArgs : EventArgs
{
    // required : l'ordinateur doit être fourni à la création de l'événement
    public required Ordinateur Ordinateur { get; set; }
}

public class OrdinateurSupprimeEventArgs : EventArgs
{
    public int IdSupprime { get; set; }
}

public class OrdinateurMisAJourEventArgs : EventArgs
{
    public required Ordinateur Ordinateur { get; set; }
}
