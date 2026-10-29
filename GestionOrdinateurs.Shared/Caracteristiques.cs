namespace GestionOrdinateurs.Shared;

// caractéristiques techniques, stockées dans la table Ordinateur (type possédé EF Core)
public class Caracteristiques
{
    public int RamGo { get; set; }
    public TypeOrdinateur TypeOrdi { get; set; }
    public int StockageGo { get; set; }
    public string Marque { get; set; } = string.Empty;
    public string Processeur { get; set; } = string.Empty;

    public Caracteristiques() { }

    public Caracteristiques(int ramGo, int stockageGo, TypeOrdinateur typeOrdi, string marque, string processeur)
    {
        RamGo = ramGo;
        StockageGo = stockageGo;
        TypeOrdi = typeOrdi;
        Marque = marque;
        Processeur = processeur;
    }
}
