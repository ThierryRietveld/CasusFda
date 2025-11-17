using Application;
using CasusFda;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");
builder.Configuration.AddJsonFile("appsettings.Infrastructure.json");

builder.Configuration.AddUserSecrets<Program>(optional: true);

var fundaOptions = builder.Configuration
    .GetSection(FundaOptions.Section)
    .Get<FundaOptions>() ?? throw new Exception("Could not instantiate FundaOptions");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(fundaOptions);
builder.Services.AddConsole();

IHost host = builder.Build();
await host.RunAsync();
