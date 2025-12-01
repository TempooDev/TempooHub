var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.TempooHub_Api>("tempoohub-api");

builder.AddJavaScriptApp("tempoohub-web", "../TempooHub.Web", runScriptName: "start")
    .WithReference(api)
    .WaitFor(api)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
