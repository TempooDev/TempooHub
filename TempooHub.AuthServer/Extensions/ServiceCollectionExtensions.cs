using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TempooHub.AuthServer.Data;
using TempooHub.AuthServer.Services;

namespace TempooHub.AuthServer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAuthDatabase(this WebApplicationBuilder builder)
        {
            var services = builder.Services;
            var configuration = builder.Configuration;
            var authority = configuration["Authentication:Authority"];
            var jwtKey = configuration["Authentication:JwtKey"];

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("tempoohub-auth-db"));
            });

            // 1. Identity configura sus esquemas (Cookies es el primario para IdentityApiEndpoints)
            services.AddIdentityApiEndpoints<IdentityUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // 2. Agregamos soporte para JWT sin hacerlo el "Default" global, 
            // para que no pise a las Cookies de Angular.
            services.AddAuthentication(options =>
                { })
                .AddJwtBearer(options =>
                {
                    options.Authority = authority;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = authority,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAuthorization(options =>
    {
        // Creamos una política que acepte AMBOS esquemas por defecto
        var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
            JwtBearerDefaults.AuthenticationScheme,
            IdentityConstants.ApplicationScheme);

        defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
        options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
    });

            // 4. Ajuste de la Cookie para Razor Pages
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Admin/Login";
                options.AccessDeniedPath = "/Admin/Login";
                options.Cookie.Name = "TempooHub.Admin.Auth";
            });
        }

        public static void AddAuthCors(this WebApplicationBuilder builder)
        {
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                         ?? new[] { "http://localhost:4200" }; // Fallback por defecto

            var services = builder.Services;
            services.AddCors(options =>
            {
                options.AddPolicy("AngularPolicy", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });
        }

        public static void AddAuthInfrastructure(this WebApplicationBuilder builder)
        {
            var services = builder.Services;
            var configuration = builder.Configuration;

            services.AddRazorPages();

            // // Configure SMTP email sender (optional). Fill Smtp section in appsettings.json or environment variables.
            // services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
            // services.AddTransient<IEmailSender, SmtpEmailSender>();

            services.AddControllers();
            services.AddAuthorization();

            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            services.AddOpenApi();
        }

        // Convenience method (keeps backward compatibility)
        public static void AddAuthServerServices(this WebApplicationBuilder builder)
        {
            builder.AddAuthDatabase();
            builder.AddAuthCors();
            builder.AddAuthInfrastructure();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }
    }
}
