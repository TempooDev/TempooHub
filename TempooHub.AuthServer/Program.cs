var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
// Register services in separated groups
builder.AddAuthDatabase();
builder.AddAuthCors();
builder.AddAuthInfrastructure();

var app = builder.Build();

app.UseAuthServerPipeline();

// Map auth endpoints (moved to EndpointExtensions)
app.MapAuthEndpoints();

app.UseHttpsRedirection();

// Run migrations via extension
app.MigrateDatabase();

app.Run();

public record RoleAssignmentRequest(string Email, string RoleName);