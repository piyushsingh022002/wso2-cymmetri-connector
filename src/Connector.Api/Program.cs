using Connector.Connectors;
using Connector.Core.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpClient<Wso2Client>(c => {
c.BaseAddress = new Uri(builder.Configuration["Wso2:BaseUrl"]);
// add auth header via DelegatingHandler or default creds
});
builder.Services.AddHttpClient<CymmetriClient>(c => {
c.BaseAddress = new Uri(builder.Configuration["Cymmetri:BaseUrl"]);
// add Basic Auth header using credentials from config for now
});