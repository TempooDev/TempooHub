using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using TempooHub.AuthServer.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure EF Core with Postgres (uses the tempoohub-db from AppHost)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Try common Aspire-provided connection keys, then fallback to DefaultConnection and a reasonable default.
    var configuration = builder.Configuration;
    var cs = configuration.GetConnectionString("tempoohub-db")
          ?? configuration.GetConnectionString("postgres-server")
          ?? configuration.GetConnectionString("DefaultConnection")
          // Aspire sometimes exposes connection under Aspire:Databases:<name>:ConnectionString
          ?? configuration["Aspire:Databases:tempoohub-db:ConnectionString"]
          ?? configuration["Aspire:Databases:postgres-server:ConnectionString"]
          ?? "Host=postgres-server;Database=tempoohub-db;Username=tempoohub;Password=tempoohub";

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
}

app.Run();
