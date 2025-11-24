using Connector.Connectors.Wso2;
using Connector.Connectors.Cymmetri;
using Microsoft.Extensions.Hosting;

public class SyncWorker : BackgroundService
{
    private readonly Wso2Client _wso2;
    private readonly CymmetriClient _cym;

    public SyncWorker(Wso2Client wso2, CymmetriClient cym)
    {
        _wso2 = wso2;
        _cym = cym;
    }

    // Expose a public method for tests to invoke the background execution loop
    public Task RunExecuteAsync(CancellationToken cancellationToken) => ExecuteAsync(cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                Console.WriteLine("Fetching from wso2.... .... ....");

                var users = await _wso2.GetUsersAsync();

                Console.WriteLine($"Fetched {users.Count} users.");

                foreach (var u in users)
                {
                    Console.WriteLine($"Sending user {u.Username} to Cymmetri...");

                    var resp = await _cym.CreateUserAsync(u);
                    var body = await resp.Content.ReadAsStringAsync();

                    if (resp.IsSuccessStatusCode)
                        Console.WriteLine($"? Created {u.Username} successfully");
                    else
                        Console.WriteLine($"? Failed to create {u.Username}: {resp.StatusCode} - {body}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Background service error: {ex.Message}");
                // optionally add retry or delay
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
