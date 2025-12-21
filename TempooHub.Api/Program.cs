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
        // Point to the AuthServer's discovery endpoint
        options.Authority = "http://tempoohub-auth:5001";
        options.RequireHttpsMetadata = false; // development only

        // Expect tokens intended for this API
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = "tempoohub-api",
            ValidateIssuer = true,
            // Use the standard Role claim
            RoleClaimType = ClaimTypes.Role
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
