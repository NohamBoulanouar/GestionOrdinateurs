namespace GestionOrdinateurs.API.Services;

public class StockService
{
    private readonly object _verrou = new();

    public int StockSSD { get; private set; } = 10;

    // diminue le stock de 1 ; le verrou évite que deux threads modifient StockSSD en même temps
    public void ConsommerSSD()
    {
        Task.Delay(10).Wait();

        lock (_verrou)
        {
            StockSSD--;
        }
    }
}
