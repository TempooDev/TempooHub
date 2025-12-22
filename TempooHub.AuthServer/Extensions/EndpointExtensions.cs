using System.Security.Claims;

namespace TempooHub.AuthServer.Extensions
{
    public static class EndpointExtensions
    {
        public static void MapDocsAndApis(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.MapOpenApi();
            }

            app.MapGet("/", () => Results.Ok(new { message = "TempooHub AuthServer (OpenIddict)" }));
            app.MapIdentityApi<IdentityUser>();
            app.MapScalarApiReference("/api-docs");
            app.MapScalarApiReference("/docs");
        }

        public static void MapAuthEndpoints(this WebApplication app)
        {
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
        }
    }
}
