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
                        ValidateIssuer = true
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

app.UseHttpsRedirection();

app.Run();

public record ResultObject(string message);
