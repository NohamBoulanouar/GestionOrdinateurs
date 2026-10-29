using System.Text;
using System.Text.Json.Serialization;
using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.API.Middleware;
using GestionOrdinateurs.API.Services;
using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Ordinateur -> Reparations -> UnOrdinateur : on évite la boucle infinie en JSON
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    // les enums sont affichés et saisis en texte (ex : "Lent") dans Swagger
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddScoped<IOrdinateurService, OrdinateursService>();
builder.Services.AddScoped<ReparationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<StockService>(); // singleton : le stock est partagé entre les requêtes
builder.Services.AddSingleton<JournalService>(); // singleton : un seul verrou pour les fichiers de logs
builder.Services.AddDbContext<ParcInformatiqueContext>();
builder.Services.AddEndpointsApiExplorer();

// CORS : le client Blazor passe maintenant par la Gateway, c'est donc elle qui appelle l'API
// (si l'API autorisait aussi le client, l'en-tête CORS serait en double avec celui de la Gateway)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGateway", policy =>
    {
        policy.WithOrigins("http://localhost:5010") // URL de la Gateway
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Swagger avec le bouton "Authorize" pour saisir le token JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GestionOrdinateurs",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entrez : Bearer {token}"
    });

    // envoie le token sur toutes les requêtes faites depuis Swagger
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

// Configuration JWT : même clé que celle utilisée par AuthentificationApi pour signer
var jwtKey = builder.Configuration["Jwt:Key"] ?? "";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            Console.WriteLine($"Utilisateur utilisant un Token Valide --- {context.Principal?.Identity?.Name ?? "Anonymous"}");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("Token Invalide");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

//Initialisation de la base de données
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ParcInformatiqueContext>();

    // applique les migrations en attente (table Users, colonne Vendu...)
    db.Database.Migrate();

    // ajoute quelques utilisateurs si la table est vide
    if (!db.Users.Any())
    {
        db.Users.AddRange(
            new User { Login = "user1", Role = RoleEnum.Admin },
            new User { Login = "user2", Role = RoleEnum.Gestionnaire },
            new User { Login = "user3", Role = RoleEnum.Gestionnaire }
        );

        db.SaveChanges();
    }
}

// en premier : attrape les exceptions de tout le reste du pipeline
app.UseMiddleware<ExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // accessible sur /swagger
}

app.UseRouting();

//Activer CORS
app.UseCors("AllowGateway");

// Authentification & Autorisation
app.UseAuthentication();
app.UseMiddleware<RequestLoggerMiddleware>(); // après l'authentification pour connaître l'utilisateur
app.UseAuthorization();

app.MapControllers();

app.Run();
