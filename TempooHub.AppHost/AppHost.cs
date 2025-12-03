var builder = DistributedApplication.CreateBuilder(args);

var user = builder.AddParameter("tempoohub-user");
var pass = builder.AddParameter("tempoohub-password", true);
var dbServer = builder.AddPostgres("postgres-server")
    .WithPgAdmin()
    .WithUserName(user)
    .WithPassword(pass)
    .WithLifetime(ContainerLifetime.Persistent);

var tempooHubDb = dbServer.AddDatabase("tempoohub-db");

var keycloakUser = builder.AddParameter("keycloak-user");
var keycloakPass = builder.AddParameter("keycloak-pass", secret:true);

var keycloak = builder
    .AddKeycloak("keycloak", 8080, keycloakUser, keycloakPass)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var api = builder.AddProject<Projects.TempooHub_Api>("tempoohub-api")
        .WithReference(keycloak)
        .WaitFor(keycloak)
        .WithReference(tempooHubDb)
        .WaitFor(tempooHubDb);

builder.AddJavaScriptApp("tempoohub-web", "../TempooHub.Web", runScriptName: "start")
    .WithReference(api)
    .WaitFor(api)
    .WithHttpEndpoint(env: "PORT",port: 4200)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
