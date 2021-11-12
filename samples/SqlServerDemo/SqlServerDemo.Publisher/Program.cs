﻿using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NTangle;
using NTangle.Data;
using NTangle.Events;
using NTangle.Services;

namespace SqlServerDemo.Publisher
{
    /// <summary>
    /// The console program leveraging <see href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host"/>.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">The console arguments.</param>
        internal static void Main(string[] args) => CreateHostBuilder(args).ConfigureHostConfiguration(c => c.AddEnvironmentVariables(prefix: "SqlServerDemo")).Build().Run();

        internal static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(configure => configure.AddConsole())
                            .AddScoped<IDatabase>(sp => new Database(() => new SqlConnection(hostContext.Configuration.GetConnectionString("SqlDb"))))
                            .AddSingleton<IIdentifierGenerator<string>, IdentifierGenerator>()
                            .AddScoped<IEventSerializer, CloudEventSerializer>()
                            .AddScoped<IEventPublisher, LoggerEventPublisher>()
                            .AddScoped<IHostedServiceSynchronizer, FileLockSynchronizer>()
                            .AddGeneratedOrchestratorServices()
                            .AddGeneratedHostedServiceServices(hostContext.Configuration);
                });
    }
}