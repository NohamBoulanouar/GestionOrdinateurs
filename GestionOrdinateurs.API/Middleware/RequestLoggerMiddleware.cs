namespace GestionOrdinateurs.API.Middleware;

// trace chaque opération effectuée : utilisateur connecté, verbe HTTP, chemin et code retour
// (placé après UseAuthentication pour connaître l'utilisateur du token)
public class RequestLoggerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggerMiddleware> _logger;

    public RequestLoggerMiddleware(RequestDelegate next, ILogger<RequestLoggerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string utilisateur = context.User.Identity?.Name ?? "Anonyme";

        await _next(context);

        _logger.LogInformation("[TRACE] {Utilisateur} - {Methode} {Chemin} -> {Code}",
            utilisateur, context.Request.Method, context.Request.Path, context.Response.StatusCode);
    }
}
