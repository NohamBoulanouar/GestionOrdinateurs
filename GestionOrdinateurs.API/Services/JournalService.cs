namespace GestionOrdinateurs.API.Services;

// écrit les actions dans des fichiers texte (Logs\Reparations et Logs\Ordinateurs)
public class JournalService
{
    private const string DossierReparations = "Logs/Reparations";
    private const string DossierOrdinateurs = "Logs/Ordinateurs";

    // plusieurs réparations peuvent écrire en même temps : un seul accès au fichier à la fois
    private readonly object _verrou = new();

    public JournalService()
    {
        Directory.CreateDirectory(DossierReparations);
        Directory.CreateDirectory(DossierOrdinateurs);
    }

    public void EcrireReparation(string message) =>
        Ecrire(Path.Combine(DossierReparations, "reparations.log"), message);

    public void EcrireOrdinateur(string message) =>
        Ecrire(Path.Combine(DossierOrdinateurs, "ordinateurs.log"), message);

    private void Ecrire(string chemin, string message)
    {
        lock (_verrou)
        {
            // using : le fichier est fermé (Dispose) dès la fin du bloc
            using var writer = new StreamWriter(chemin, append: true);
            writer.WriteLine($"{DateTime.Now:dd/MM/yyyy} - {message}");
        }
    }
}
