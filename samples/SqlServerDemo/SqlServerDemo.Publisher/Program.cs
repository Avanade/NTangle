using CoreEx.Configuration;
using CoreEx.Events;
using DbEx;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NTangle.Data.SqlServer;
using SqlServerDemo.Publisher.Data;

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
        internal static void Main(string[] args) => CreateHostBuilder(args).ConfigureHostConfiguration(c => c.AddEnvironmentVariables(prefix: "SqlServerDemo_")).Build().Run();

        /// <summary>
        /// Create the <see cref="IHostBuilder"/>.
        /// </summary>
        internal static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var settings = new DefaultSettings(hostContext.Configuration);

                    services.AddDefaultSettings()
                            .AddLogging(configure => configure.AddConsole())
                            .AddDatabase(sp => new SqlServerDatabase(() => new SqlConnection(hostContext.Configuration.GetConnectionString("SqlDb"))))
                            .AddStringIdentifierGenerator()
                            .AddJsonSerializer();

                    // Adds the EventPublisher, which will use the default EventDataFormatter, with CloudEventSerializer and EventOutbox enqueue as sender.
                    services.AddEventPublisher()
                            .AddEventDataFormatter()
                            .AddCloudEventSerializer()
                            .AddGeneratedEventOutboxSender();

                    // Adds the CDC-hosted service(s) including orchestrator services, and specified EventOutbox dequeue/send service.
                    services.AddGeneratedCdcHostedServices(settings)
                            .AddEventOutboxHostedService(settings, sp => new EventOutboxDequeue(sp.GetService<IDatabase>(), new LoggerEventSender(sp.GetService<ILogger<LoggerEventSender>>()), sp.GetService<ILogger<EventOutboxDequeue>>()))
                            .AddGeneratedOrchestratorServices()
                            .AddFileLockSynchronizer();
                });
    }
}