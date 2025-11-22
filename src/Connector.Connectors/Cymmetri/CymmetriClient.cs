using System.Net.Http.Json;
using Connector.Core.Models;


public class CymmetriClient
{
private readonly HttpClient _http;
public CymmetriClient(HttpClient http) => _http = http;


public async Task<HttpResponseMessage> CreateUserAsync(CanonicalUser u)
{
// Build SCIM user payload; set externalId to SourceId
var payload = new {
userName = u.Username,
name = new { givenName = u.GivenName, familyName = u.FamilyName },
emails = new[] { new { value = u.Email, primary = true } },
externalId = u.SourceId
};
return await _http.PostAsJsonAsync("/scim2/Users", payload);
}
}