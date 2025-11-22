using Connector.Core.Models;
using Microsoft.Extensions.Hosting;


public class SyncWorker : BackgroundService
{
private readonly Wso2Client _wso2;
private readonly CymmetriClient _cym;
public SyncWorker(Wso2Client wso2, CymmetriClient cym) { _wso2 = wso2; _cym = cym; }


protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
while (!stoppingToken.IsCancellationRequested)
{
var users = await _wso2.GetUsersAsync();
foreach (var u in users)
{
var resp = await _cym.CreateUserAsync(u);
if (!resp.IsSuccessStatusCode)
{
// log and handle errors (retry later) - for prototype just write console
Console.WriteLine($"Failed to create {u.Username}: {resp.StatusCode}");
}
}


await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
}
}
}