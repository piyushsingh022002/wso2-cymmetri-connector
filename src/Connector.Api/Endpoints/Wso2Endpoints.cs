using Connector.Connectors.Wso2;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Connector.Api.Endpoints
{ 
    public static class Wso2Endpoints
    {
        public static void MapWso2Endpoints(this WebApplication app)
        {
            app.MapGet("/test-wso2", async (Wso2Client wso2Client) =>
            {
                var users = await wso2Client.GetUsersAsync();
                return Results.Ok(users);
            });
        }
    }

}
