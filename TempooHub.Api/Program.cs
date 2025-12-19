using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "http://localhost:8080/realms/tempoohub";
        options.Audience = "tempoo-hub-client";
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidIssuer = "http://localhost:8080/realms/tempoohub",
            ValidateIssuer = true,
            // Importante: Esto le dice a .NET que use el tipo de claim Role estándar
            RoleClaimType = ClaimTypes.Role 
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var userPrincipal = context.Principal;
                var realmAccessClaim = userPrincipal?.FindFirst("realm_access");

                if (realmAccessClaim != null)
                {
                    // Parseamos el JSON de realm_access: {"roles": ["admin", "..."]}
                    using var doc = JsonDocument.Parse(realmAccessClaim.Value);
                    if (doc.RootElement.TryGetProperty("roles", out var rolesElement))
                    {
                        if (userPrincipal?.Identity is ClaimsIdentity identity)
                        {
                            foreach (var role in rolesElement.EnumerateArray())
                            {
                                // Agregamos cada rol como un Claim de tipo Role real
                                identity.AddClaim(new Claim(ClaimTypes.Role, role.GetString()!));
                            }
                        }
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    ;

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => Results.Ok(new ResultObject("Okey")));

app.MapGet("/user", () => Results.Ok(new ResultObject("Autenticado")))
    .RequireAuthorization();

app.MapGet("/admin", () => Results.Ok(new ResultObject("Administrador")))
    .RequireAuthorization(policy => policy.RequireRole("admin"));

app.UseHttpsRedirection();

app.Run();

public record ResultObject(string message);
