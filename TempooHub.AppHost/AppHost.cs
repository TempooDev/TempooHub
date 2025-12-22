var builder = DistributedApplication.CreateBuilder(args);

// Parámetros y Base de Datos
var user = builder.AddParameter("tempoohub-user");
var pass = builder.AddParameter("tempoohub-password", true);

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

// 2. API de Negocio (SaaS)
var api = builder.AddProject<Projects.TempooHub_Api>("tempoohub-api")
    .WithReference(tempooHubDb)
    // Esto inyecta la URL del authServer para la validación JWT
    .WithReference(authServer) 
    .WaitFor(tempooHubDb);

// 3. Gateway (YARP)
// Es el único que necesita ser externo si Angular vive en el mismo dominio de red
var gateway = builder.AddProject<Projects.TempooHub_Gateway>("gateway")
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

// Configuración de CORS dinámica para el AuthServer
// Ahora el origen permitido es el de la App de Angular
authServer.WithEnvironment("AllowedOrigins__0", angularApp.GetEndpoint("http"));

builder.Build().Run();