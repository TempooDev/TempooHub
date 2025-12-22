var builder = DistributedApplication.CreateBuilder(args);

var user = builder.AddParameter("tempoohub-user");
var pass = builder.AddParameter("tempoohub-password", true);
var dbServer = builder.AddPostgres("postgres-server")
    .WithPgAdmin()
    .WithUserName(user)
    .WithPassword(pass)
    .WithLifetime(ContainerLifetime.Persistent);

var tempooHubDb = dbServer.AddDatabase("tempoohub-db");

var tempooHubAuthDb = dbServer.AddDatabase("tempoohub-auth-db");

var authServer = builder.AddProject<Projects.TempooHub_AuthServer>("tempoohub-auth")
    .WithReference(tempooHubAuthDb)
    .WaitFor(tempooHubAuthDb)
    ;

builder.AddJavaScriptApp("tempoohub-web", "../TempooHub.Web", runScriptName: "start")
    .WithReference(authServer)
    .WaitFor(authServer)
    .WithEnvironment("API_URL", authServer.GetEndpoint("http"))
    .WithHttpEndpoint(env: "PORT",port: 4200)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
