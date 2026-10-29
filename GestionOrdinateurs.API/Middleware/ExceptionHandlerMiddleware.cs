using System.Text.Json;

namespace GestionOrdinateurs.API.Middleware;

// intercepte les exceptions non gérées et renvoie une erreur 500 en JSON
public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur non gérée sur {Methode} {Chemin}", context.Request.Method, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            // JsonSerializer échappe correctement le message (guillemets, etc.)
            string reponse = JsonSerializer.Serialize(new { erreur = ex.Message, date = DateTime.UtcNow });
            await context.Response.WriteAsync(reponse);
        }
    }
}
