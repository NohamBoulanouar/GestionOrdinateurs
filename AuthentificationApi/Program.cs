using System.Text.Json.Serialization;
using AuthentificationApi.Services;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    // les enums (RoleEnum) sont lus et écrits en texte
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AuthentificationApi",
        Version = "v1"
    });
});

// HttpClientFactory : client nommé vers GestionOrdinateurs.API (url dans appsettings.json)
builder.Services.AddHttpClient(UserApiService.NomClient, client =>
{
    client.BaseAddress = new Uri(builder.Configuration["GestionOrdinateursApi:BaseUrl"] ?? "");
});

builder.Services.AddScoped<IUserApiService, UserApiService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// CORS : seul le client Blazor peut appeler cette API depuis le navigateur
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        // URL du client Blazor : 5000 sous docker (Cors:ClientOrigin), 5095 en local par défaut
        policy.WithOrigins(builder.Configuration["Cors:ClientOrigin"] ?? "http://localhost:5095")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // accessible sur /swagger
}

//Activer CORS
app.UseCors("AllowBlazorClient");

app.MapControllers();

app.Run();
