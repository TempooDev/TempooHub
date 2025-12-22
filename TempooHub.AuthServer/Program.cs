using TempooHub.AuthServer.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
// Register services in separated groups
builder.AddAuthDatabase();
builder.AddAuthCors();
builder.AddAuthInfrastructure();

var app = builder.Build();

app.UseAuthServerPipeline();

app.UseHttpsRedirection();

// Run migrations via extension
app.MigrateDatabase();

app.Run();

public record RoleAssignmentRequest(string Email, string RoleName);