using GestionOrdinateurs.Gateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

// YARP : les routes et l'adresse de GestionOrdinateurs.API sont dans la section ReverseProxy de appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// CORS : le client Blazor appelle la Gateway depuis le navigateur
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5095") // URL de ton client Blazor
              .AllowAnyHeader() // laisse passer l'en-tête Authorization (token JWT)
              .AllowAnyMethod();
    });
});

var app = builder.Build();

//Activer CORS
app.UseCors("AllowBlazorClient");

app.UseMiddleware<RequestLoggerMiddleware>();

// le header Authorization est transmis tel quel à l'API, qui valide elle-même le token
app.MapReverseProxy();

app.MapGet("/", () => "Gateway GestionOrdinateurs démarrée");

app.Run();
