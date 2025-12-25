using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

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
                UserManager<IdentityUser> userManager,
                RoleManager<IdentityRole> roleManager) =>
            {
                var userId = userManager.GetUserId(claimsUser);
                if (userId == null) return Results.Conflict();

                var user = await userManager.FindByIdAsync(userId);
                if (user == null) return Results.NotFound();

                var roles = await userManager.GetRolesAsync(user);
                var claims = new List<Claim>();

                foreach (var roleName in roles)
                {
                    var role = await roleManager.FindByNameAsync(roleName);
                    if (role != null)
                    {
                        claims.AddRange(await roleManager.GetClaimsAsync(role));
                    }
                }

                return Results.Ok(new
                {
                    email = user.Email,
                    roles = roles,
                    claims = claims.Select(c => new { c.Type, c.Value })
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

            app.MapPost("/auth/login", async (LoginRequest login, UserManager<IdentityUser> userManager, IConfiguration config) =>
            {
                var user = await userManager.FindByEmailAsync(login.Email);
                if (user != null && await userManager.CheckPasswordAsync(user, login.Password))
                {
                    var roles = await userManager.GetRolesAsync(user);
                    var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Authentication:JwtKey"]!));
                    var creds = new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha256);

                    var claims = new List<Claim> {
                        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                        new Claim(JwtRegisteredClaimNames.Email, user.Email!)
                    };
                    foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));

                    var token = new JwtSecurityToken(
                        issuer: config["Authentication:Authority"],
                        audience: config["Authentication:Audience"],
                        claims: claims,
                        expires: DateTime.Now.AddHours(3),
                        signingCredentials: creds
                    );

                    return Results.Ok(new { accessToken = new JwtSecurityTokenHandler().WriteToken(token) });
                }
                return Results.Unauthorized();
            });

            app.MapPost("/logout", async (SignInManager<IdentityUser> signInManager) =>
            {
                await signInManager.SignOutAsync();
                return Results.Ok();
            }).RequireAuthorization();
        }
    }
}
