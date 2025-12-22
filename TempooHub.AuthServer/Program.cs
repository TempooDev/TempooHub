using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TempooHub.AuthServer.Data;
using Scalar.AspNetCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure EF Core with Postgres (uses the tempoohub-db from AppHost)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var configuration = builder.Configuration;
    var cs = configuration.GetConnectionString("tempoohub-auth-db");

    options.UseNpgsql(cs);
});

builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllers();
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // URL de tu Angular
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // REQUERIDO para enviar Cookies
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCors("AngularPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
}
app.MapGet("/", () => Results.Ok(new { message = "TempooHub AuthServer (OpenIddict)" }));
app.MapIdentityApi<IdentityUser>();
app.MapScalarApiReference("/api-docs");
app.MapScalarApiReference("/docs");


#region auth
app.MapGet("/manage/user-details", async (
    ClaimsPrincipal claimsUser,
    UserManager<IdentityUser> userManager) =>
{
    var userId = claimsUser.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId == null) return Results.Unauthorized();

    var user = await userManager.FindByIdAsync(userId);
    if (user == null) return Results.NotFound();

    var roles = await userManager.GetRolesAsync(user);

    return Results.Ok(new
    {
        email = user.Email,
        roles = roles
    });
}).RequireAuthorization();

app.MapPost("/setup-role", async (
    RoleAssignmentRequest request,
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager) =>
{
    if (!await roleManager.RoleExistsAsync(request.RoleName))
    {
        await roleManager.CreateAsync(new IdentityRole(request.RoleName));
    }

    var user = await userManager.FindByEmailAsync(request.Email);
    if (user == null) return Results.NotFound("Usuario no encontrado");

    await userManager.AddToRoleAsync(user, request.RoleName);
    return Results.Ok($"Rol {request.RoleName} asignado");
});
app.MapPost("/logout", async (SignInManager<IdentityUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Ok();
}).RequireAuthorization();
#endregion
app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        if (context.Database.IsRelational())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al aplicar las migraciones.");
    }
}

app.Run();

public record RoleAssignmentRequest(string Email, string RoleName);