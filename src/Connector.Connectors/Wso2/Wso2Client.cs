using System.Net.Http.Json;


public class Wso2Client
{
private readonly HttpClient _http;
public Wso2Client(HttpClient http) => _http = http;


public async Task<List<CanonicalUser>> GetUsersAsync(int startIndex = 1, int count = 50)
{
// GET /scim2/Users?startIndex=1&count=50
var url = $"/scim2/Users?startIndex={startIndex}&count={count}";
var res = await _http.GetAsync(url);
res.EnsureSuccessStatusCode();
var body = await res.Content.ReadFromJsonAsync<JsonElement>();
// naive mapping: extract Resources[] and map a few fields
var list = new List<CanonicalUser>();
if (!body.TryGetProperty("Resources", out var resources)) return list;
foreach (var r in resources.EnumerateArray())
{
var u = new CanonicalUser{
SourceId = r.GetProperty("id").GetString() ?? string.Empty,
Username = r.GetProperty("userName").GetString() ?? string.Empty,
GivenName = r.GetProperty("name").GetProperty("givenName").GetString() ?? string.Empty,
FamilyName = r.GetProperty("name").GetProperty("familyName").GetString() ?? string.Empty,
Email = r.GetProperty("emails").EnumerateArray().First().GetProperty("value").GetString() ?? string.Empty
};
list.Add(u);
}
return list;
}
}