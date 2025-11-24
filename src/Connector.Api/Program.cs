using Connector.Api.AuthCustomMiddleware;
using Connector.Api.Endpoints;
using Connector.Connectors.Cymmetri;
using Connector.Connectors.Wso2;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Validate configuration
    var wso2BaseUrl = builder.Configuration["Wso2:BaseUrl"];
    if (string.IsNullOrWhiteSpace(wso2BaseUrl))
        throw new Exception("Missing Wso2:BaseUrl in configuration.");

    var cymBaseUrl = builder.Configuration["Cymmetri:BaseUrl"];
    if (string.IsNullOrWhiteSpace(cymBaseUrl))
        throw new Exception("Missing Cymmetri:BaseUrl in configuration.");

    // Add HttpClients with SSL bypass for dev
    builder.Services.AddHttpClient<Wso2Client>(c =>
    {
        c.BaseAddress = new Uri("https://localhost:9443/");
        var byteArray = Encoding.ASCII.GetBytes("admin:admin");
        c.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

    builder.Services.AddHttpClient<CymmetriClient>(c =>
    {
        c.BaseAddress = new Uri(builder.Configuration["Cymmetri:BaseUrl"]);
    })
.ConfigurePrimaryHttpMessageHandler(() =>
    new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    })
.AddTypedClient((http, sp) =>
    new CymmetriClient(
        http,
        builder.Configuration["SCIM:ConnectorToken"]
    )
);


    // Register background service
    builder.Services.AddHostedService<SyncWorker>();

    var app = builder.Build();

    // Register custom middleware
    app.UseMiddleware<AuthMiddleware>();

    // Minimal endpoint to keep host running
    app.MapGet("/", () => "Connector API is running.");

    // Register Wso2 endpoints
    app.MapWso2Endpoints();

    // Optional: Run SyncWorker once in background without blocking startup
    _ = Task.Run(async () =>
    {
        using var scope = app.Services.CreateScope();
        var wso2Client = scope.ServiceProvider.GetRequiredService<Wso2Client>();
        var cymClient = scope.ServiceProvider.GetRequiredService<CymmetriClient>();

        var worker = new SyncWorker(wso2Client, cymClient);
        await worker.RunExecuteAsync(CancellationToken.None);
    });

    // Start the web app
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("Startup exception:");
    Console.WriteLine(ex);
    throw;
}
