using System.Net.Http.Headers;
using System.Net.Http.Json;
using Connector.Core.Models;

namespace Connector.Connectors.Cymmetri
{

    public class CymmetriClient
    {
        private readonly HttpClient _http;

        public CymmetriClient(HttpClient http, string token)
        {
            _http = http;

            // Use Bearer token for Cymmetri SCIM auth
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<HttpResponseMessage> CreateUserAsync(CanonicalUser u)
        {
            var payload = new
            {
                userName = u.Username,
                name = new { givenName = u.GivenName, familyName = u.FamilyName },
                emails = new[] { new { value = u.Email, primary = true } },
                externalId = u.SourceId
            };

            return await _http.PostAsJsonAsync("/scim2/Users", payload);
        }
    }
}
