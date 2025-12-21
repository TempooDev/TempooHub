using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using TempooHub.AuthServer.Data;
using OpenIddict.EntityFrameworkCore.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

// Configure EF Core with Postgres (uses the tempoohub-db from AppHost)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Try common Aspire-provided connection keys, then fallback to DefaultConnection and a reasonable default.
    var configuration = builder.Configuration;
        var cs = configuration.GetConnectionString("tempoohub-auth-db")
            ?? configuration.GetConnectionString("postgres-auth")
            ?? configuration.GetConnectionString("DefaultConnection")
            // Aspire sometimes exposes connection under Aspire:Databases:<name>:ConnectionString
            ?? configuration["Aspire:Databases:tempoohub-auth-db:ConnectionString"]
            ?? configuration["Aspire:Databases:postgres-auth:ConnectionString"]
            ?? "Host=postgres-auth;Database=tempoohub-auth-db;Username=tempoohub-auth;Password=tempoohub-auth";

    options.UseNpgsql(cs);

    // Register the OpenIddict entity sets.
    options.UseOpenIddict();
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("/connect/token");
        options.SetAuthorizationEndpointUris("/connect/authorize");

        options.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();
        options.AllowClientCredentialsFlow();

        options.AcceptAnonymousClients();

        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough()
               .EnableAuthorizationEndpointPassthrough();

        options.DisableAccessTokenEncryption();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.AddControllers();

// CORS for the SPA
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .WithOrigins("http://localhost:4200", "http://tempoohub-web:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { message = "TempooHub AuthServer (OpenIddict)" }));
app.MapControllers();

// Ensure DB created and seeded minimally
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var testUser = await userManager.FindByNameAsync("admin");
    if (testUser == null)
    {
        var admin = new IdentityUser("admin");
        await userManager.CreateAsync(admin, "Admin123!");
    }

    // Seed OpenIddict clients
    var appManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

    if (await appManager.FindByClientIdAsync("tempoohub-api") == null)
    {
        // Read secret from environment (populated by AppHost) or fallback to seeded value
        var apiSecret = Environment.GetEnvironmentVariable("TEMPOOHUB_API_SECRET") ?? "tempoohub-api-secret";

        await appManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "tempoohub-api",
            ClientSecret = apiSecret,
            DisplayName = "TempooHub API",
            Permissions =
            {
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.ClientCredentials,
                Permissions.Prefixes.Scope + "api"
            }
        });
    }

    if (await appManager.FindByClientIdAsync("tempoohub-web") == null)
    {
        await appManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "tempoohub-web",
            DisplayName = "TempooHub Web SPA",
            RedirectUris = { new Uri("http://localhost:4200/"), new Uri("http://tempoohub-web:4200/") , new Uri("http://localhost:4200/silent-refresh.html"), new Uri("http://tempoohub-web:4200/silent-refresh.html")},
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.Logout,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Prefixes.Scope + "openid",
                Permissions.Prefixes.Scope + "profile",
                Permissions.Prefixes.Scope + "email",
                Permissions.Prefixes.Scope + "api"
            }
        });
    }
}

app.Run();
