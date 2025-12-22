var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(); 

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapDefaultEndpoints(); // Health checks de Aspire
app.MapReverseProxy();
app.Run();