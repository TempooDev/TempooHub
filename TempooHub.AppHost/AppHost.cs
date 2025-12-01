var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.TempooHub_Api>("tempoohub-api");

builder.Build().Run();
