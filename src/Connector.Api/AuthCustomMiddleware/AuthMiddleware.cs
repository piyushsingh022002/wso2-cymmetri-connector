using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Connector.Api.AuthCustomMiddleware
{

    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _expectedToken;

        public AuthMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _expectedToken = config["SCIM:ConnectorToken"] ?? string.Empty;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader)
                || !authHeader.ToString().StartsWith("Bearer ")
                || authHeader.ToString().Substring(7) != _expectedToken)
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            await _next(context); // Proceed if token matches
        }
    }

}
