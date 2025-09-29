using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Running Migrations...");

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true)
    .Build();

var builder = Host.CreateDefaultBuilder(args);

var connectionString = configuration.GetConnectionString("DbConnection");

if (args.Length > 0)
{
    var connectionStringArg = args[0].Split("--connection");
    if (connectionStringArg.Length < 2)
    {
        Console.WriteLine("Missing connection argument");
    }
    else
    {
        connectionString = connectionStringArg[1].Trim();
    }

    if (string.IsNullOrEmpty(connectionString))
    {
        Console.WriteLine("Missing connection string");
    }
}

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Migrations ran failed. Missing connection string...");
    return 1;
}

builder.ConfigureServices(services =>
{
    services.AddSingleton(configuration);
    services.AddInfrastructure(connectionString);
});

var app = builder.Build();

var context = app.Services.GetRequiredService<BackgroundJobDistributedLockDbContext>();

context.Database.SetCommandTimeout(3600);
await context.Database.MigrateAsync();

Console.WriteLine("Migrations ran successfully...");
return 0;