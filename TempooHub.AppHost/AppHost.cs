var builder = DistributedApplication.CreateBuilder(args);

var user = builder.AddParameter("tempoohub-user");
var pass = builder.AddParameter("tempoohub-password", true);
var dbServer = builder.AddPostgres("postgres-server")
    .WithPgAdmin()
    .WithUserName(user)
    .WithPassword(pass)
    .WithLifetime(ContainerLifetime.Persistent);

var tempooHubDb = dbServer.AddDatabase("tempoohub-db");

// Reemplazado Keycloak por un proyecto AuthServer que usa OpenIddict
var authServer = builder.AddProject<Projects.TempooHub_AuthServer>("tempoohub-auth")
    .WithHttpEndpoint(env: "AUTH_PORT", port: 5000)
    .WithReference(tempooHubDb)
    .WaitFor(tempooHubDb)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

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
