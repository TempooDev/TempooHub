var builder = DistributedApplication.CreateBuilder(args);

// Parámetros y Base de Datos
var user = builder.AddParameter("tempoohub-user");
var pass = builder.AddParameter("tempoohub-password", true);


var jwtKey = builder.AddParameter("jwt-key", true);

var dbServer = builder.AddPostgres("postgres-server")
    .WithPgAdmin()
    .WithUserName(user)
    .WithPassword(pass)
    .WithLifetime(ContainerLifetime.Persistent);

var tempooHubDb = dbServer.AddDatabase("tempoohub-db");
var tempooHubAuthDb = dbServer.AddDatabase("tempoohub-auth-db");

// 1. AuthServer
var authServer = builder.AddProject<Projects.TempooHub_AuthServer>("tempoohub-auth")
    .WithReference(tempooHubAuthDb)
    .WaitFor(tempooHubAuthDb);

var api = builder.AddProject<Projects.TempooHub_Api>("tempoohub-api")
    .WithReference(tempooHubDb)
    .WithReference(authServer)
    .WaitFor(tempooHubDb);

var gateway = builder.AddProject<Projects.TempooHub_Proxy>("gateway")
    .WithReference(authServer)
    .WithReference(api)
    .WithExternalHttpEndpoints();

// 4. Angular App
var angularApp = builder.AddJavaScriptApp("tempoohub-web", "../TempooHub.Web", runScriptName: "start")
    .WithReference(gateway)
    .WaitFor(gateway)
    // Usamos el endpoint del Gateway como única entrada de API
    .WithEnvironment("API_URL", gateway.GetEndpoint("http"))
    .WithHttpEndpoint(env: "PORT", port: 4200)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

var gatewayUrl = gateway.GetEndpoint("http");
var authAuthority = $"{gatewayUrl}/api/auth";

authServer.WithEnvironment("AllowedOrigins__0", angularApp.GetEndpoint("http"))
    .WithEnvironment("Authentication__JwtKey", jwtKey)
    .WithEnvironment("Authentication__Authority", authAuthority);

api.WithEnvironment("Authentication__JwtKey", jwtKey)
    .WithEnvironment("Authentication__Authority", authAuthority);

gateway.WithEnvironment("AllowedOrigins__0", angularApp.GetEndpoint("http"));

builder.Build().Run();