namespace GestionOrdinateurs.Gateway.Middleware;

// trace chaque requête qui passe par la Gateway, avec le code retour de l'API appelée
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
        await _next(context);

        _logger.LogInformation("[GATEWAY] {Methode} {Chemin} -> {Code}",
            context.Request.Method, context.Request.Path, context.Response.StatusCode);
    }
}
