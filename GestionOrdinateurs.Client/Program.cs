using Blazored.SessionStorage;
using GestionOrdinateurs.Client;
using GestionOrdinateurs.Client.Auth;
using GestionOrdinateurs.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// session du navigateur (stockage du token JWT)
builder.Services.AddBlazoredSessionStorage();

// authentification / autorisation côté Blazor à partir du token JWT
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddScoped<JwtHandler>();

// les URLs sont dans wwwroot/appsettings.json
string authentificationUrl = builder.Configuration["Api:AuthentificationUrl"] ?? "";
string gatewayUrl = builder.Configuration["Api:GatewayUrl"] ?? "";

// HttpClientFactory : AuthentificationApi (pas besoin de token pour se connecter)
builder.Services.AddHttpClient(ClientsHttp.Authentification, client =>
{
    client.BaseAddress = new Uri(authentificationUrl);
});

// HttpClientFactory : Gateway, le JwtHandler ajoute le token à chaque appel
builder.Services.AddHttpClient(ClientsHttp.Gateway, client =>
{
    client.BaseAddress = new Uri(gatewayUrl);
})
.AddHttpMessageHandler<JwtHandler>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ParcService>();

await builder.Build().RunAsync();
