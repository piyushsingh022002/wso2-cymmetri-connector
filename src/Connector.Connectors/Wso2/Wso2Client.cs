namespace Connector.Connectors.Wso2 {
using System.Net.Http.Json;
using System.Text.Json;
using Connector.Core.Models;

public class Wso2Client
{
 private readonly HttpClient _http;
 public Wso2Client(HttpClient http)
 {
 _http = http;
 if (_http.BaseAddress == null)
 throw new InvalidOperationException("HttpClient.BaseAddress must be set for Wso2Client.");
 }

 public async Task<List<CanonicalUser>> GetUsersAsync(int startIndex =1, int count =50)
 {
 // GET /scim2/Users?startIndex=1&count=50
 var url = $"/scim2/Users?startIndex={startIndex}&count={count}";
 var res = await _http.GetAsync(url);

            var rawBody = await res.Content.ReadAsStringAsync();
            Console.WriteLine("WSO2 Response JSON:");
            Console.WriteLine(rawBody);

            if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
 {
 // Handle 401 Unauthorized
 Console.WriteLine("Unauthorized: Check your WSO2 credentials.");
 return new List<CanonicalUser>();
 }
 if (!res.IsSuccessStatusCode)
 {
 // Handle other errors
 Console.WriteLine($"Error: {res.StatusCode} - {await res.Content.ReadAsStringAsync()}");
 return new List<CanonicalUser>();
 }

 var body = await res.Content.ReadFromJsonAsync<JsonElement>();
 // naive mapping: extract Resources[] and map a few fields
 var list = new List<CanonicalUser>();
 if (!body.TryGetProperty("Resources", out var resources)) return list;
 foreach (var r in resources.EnumerateArray())
 {
                var u = new CanonicalUser
                {
                    SourceId = r.GetProperty("id").GetString() ?? string.Empty,
                    Username = r.GetProperty("userName").GetString() ?? string.Empty,
                    GivenName = r.TryGetProperty("name", out var name) ? name.GetProperty("givenName").GetString() ?? "" : "",
                    FamilyName = r.TryGetProperty("name", out name) ? name.GetProperty("familyName").GetString() ?? "" : "",
                    Email = r.TryGetProperty("emails", out var emails) && emails.ValueKind == JsonValueKind.Array
                               ? emails[0].GetString() ?? ""
                               : ""
                };

                list.Add(u);
 }
 return list;
 }
}
}