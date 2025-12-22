using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Cargamos la configuración de YARP
var proxyBuilder = builder.Services.AddReverseProxy();

var routes = new[]
{
    new RouteConfig
    {
        RouteId = "auth-route",
        ClusterId = "auth-cluster",
        Match = new RouteMatch { Path = "/api/auth/{**remainder}" },
        Transforms = new[] { new Dictionary<string, string> { { "PathRemovePrefix", "/api/auth" } } }
    },
    new RouteConfig
    {
        RouteId = "api-route",
        ClusterId = "api-cluster",
        Match = new RouteMatch { Path = "/api/v1/{**remainder}" },
        // IMPORTANTE: Si tu API no tiene /api, este transform lo quita
        Transforms = new[] { new Dictionary<string, string> { { "PathRemovePrefix", "/api/v1" } } }
    }
};

// Definimos los CLUSTERS usando las variables de Aspire
var clusters = new[]
{
    new ClusterConfig
    {
        ClusterId = "auth-cluster",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            { "dest1", new DestinationConfig { Address = builder.Configuration["services:tempoohub-auth:https:0"] ?? builder.Configuration["services:tempoohub-auth:http:0"]! } }
        }
    },
    new ClusterConfig
    {
        ClusterId = "api-cluster",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            { "dest1", new DestinationConfig { Address = builder.Configuration["services:tempoohub-api:https:0"] ?? builder.Configuration["services:tempoohub-api:http:0"]! } }
        }
    }
};

proxyBuilder.LoadFromMemory(routes, clusters);

proxyBuilder.ConfigureHttpClient((context, handler) => {
    handler.SslOptions.RemoteCertificateValidationCallback = (s, c, l, e) => true;
});

var app = builder.Build();
app.MapReverseProxy();
app.Run();