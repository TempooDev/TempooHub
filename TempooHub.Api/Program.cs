using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

var authority = builder.Configuration["Authentication:Authority"];
var jwtKey = builder.Configuration["Authentication:JwtKey"];

// 3. Configurar Autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.RequireHttpsMetadata = false; // Solo para desarrollo con Aspire
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authority,
            ValidateAudience = false, // Permite que este token sirva para cualquier API del SaaS
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
            ClockSkew = TimeSpan.Zero // El token expira exactamente cuando dice el payload
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => Results.Ok(new ResultObject("Okey")));
app.MapGet("/data", (ClaimsPrincipal user) => 
{
    return Results.Ok(new { 
        Message = "Datos protegidos alcanzados", 
        User = user.Identity?.Name 
    });
}).RequireAuthorization();
app.Run();

public record ResultObject(string message);
