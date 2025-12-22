namespace TempooHub.AuthServer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAuthDatabase(this WebApplicationBuilder builder)
        {
            var services = builder.Services;
            var configuration = builder.Configuration;
            var authority = configuration["Authentication:Authority"]; // Ej: https://localhost:5001
            var jwtKey = configuration["Authentication:JwtKey"]; // Llave secreta compartida
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var cs = configuration.GetConnectionString("tempoohub-auth-db");
                options.UseNpgsql(cs);
            });

            services.AddIdentityApiEndpoints<IdentityUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = authority;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = authority,
                        ValidateAudience = false, // Para que cualquier SaaS pueda aceptarlo
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
                    };
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

            // Configure SMTP email sender (optional). Fill Smtp section in appsettings.json or environment variables.
            services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
            services.AddTransient<IEmailSender, SmtpEmailSender>();

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
        }
    }
}
