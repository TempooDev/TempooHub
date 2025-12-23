using TempooHub.AuthServer.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
// Register services in separated groups
builder.AddAuthServerServices();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Login"; 
    options.AccessDeniedPath = "/Admin/Login";
});

var app = builder.Build();

app.UseAuthServerPipeline();

app.UseHttpsRedirection();

// Run migrations via extension
app.MigrateDatabase();

app.Run();

public record RoleAssignmentRequest(string Email, string RoleName);