var builder = DistributedApplication.CreateBuilder(args);

var user = builder.AddParameter("tempoohub-user");
var pass = builder.AddParameter("tempoohub-password", true);
var dbServer = builder.AddPostgres("postgres-server")
    .WithPgAdmin()
    .WithUserName(user)
    .WithPassword(pass)
    .WithLifetime(ContainerLifetime.Persistent);

var tempooHubDb = dbServer.AddDatabase("tempoohub-db");

// Crear una BBDD propia para AuthServer (no referenciar tempoohub-db)
var authDbUser = builder.AddParameter("tempoohub-auth-user");
var authDbPass = builder.AddParameter("tempoohub-auth-password", secret: true);
var authDbServer = builder.AddPostgres("postgres-auth")
    .WithPgAdmin()
    .WithUserName(authDbUser)
    .WithPassword(authDbPass)
    .WithLifetime(ContainerLifetime.Persistent);

var tempooHubAuthDb = authDbServer.AddDatabase("tempoohub-auth-db");

// Reemplazado Keycloak por un proyecto AuthServer que usa OpenIddict
// Client secret for the API (seeded into AuthServer)
var tempooHubApiSecret = builder.AddParameter("tempoohub-api-secret", secret: true);

var authServer = builder.AddProject<Projects.TempooHub_AuthServer>("tempoohub-auth")
    .WithHttpEndpoint(env: "AUTH_PORT", port: 5001)
    .WithReference(tempooHubAuthDb)
    .WaitFor(tempooHubAuthDb)
    ;

var api = builder.AddProject<Projects.TempooHub_Api>("tempoohub-api")
    .WithReference(authServer)
    .WaitFor(authServer)
    .WithReference(tempooHubDb)
    .WaitFor(tempooHubDb);

builder.AddJavaScriptApp("tempoohub-web", "../TempooHub.Web", runScriptName: "start")
    .WithReference(api)
    .WaitFor(api)
    .WithHttpEndpoint(env: "PORT",port: 4200)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
