using CoreEx.Database;
using CoreEx.Database.SqlServer;
using CoreEx.Events;
#if (implement_dbprovider_sqlserver)
using Microsoft.Data.SqlClient;
#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NTangle;
using AppName.Publisher.Data;

namespace AppName.Publisher
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
        internal static void Main(string[] args) => CreateHostBuilder(args).ConfigureHostConfiguration(c => c.AddEnvironmentVariables(prefix: "AppName_")).Build().Run();

        /// <summary>
        /// Create the <see cref="IHostBuilder"/>.
        /// </summary>
        internal static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                   services.AddDefaultSettings()
                            .AddLogging(b => b.AddSimpleConsole())
#if (implement_dbprovider_sqlserver)
                            .AddDatabase(sp => new SqlServerDatabase(() => new SqlConnection(hostContext.Configuration.GetConnectionString("SqlDb"))))
#endif
                            .AddExecutionContext()
                            .AddJsonSerializer();

                    // Adds the EventPublisher, which will use the default EventDataFormatter, with CloudEventSerializer and EventOutbox enqueue as sender.
                    services.AddEventPublisher()
                            .AddEventDataFormatter()
                            .AddCloudEventSerializer();

                    // For testing purposes uses the LoggerEventSender; remove usage and uncomment others to leverage Azure Service Bus.
                    services.AddGeneratedEventOutboxSender((sp, eoe) => eoe.SetPrimaryEventSender(new LoggerEventSender(sp.GetRequiredService<ILogger<LoggerEventSender>>())));
                    //services.AddAzureServiceBusClient()
                    //        .AddGeneratedEventOutboxSender((sp, eoe) => eoe.SetPrimaryEventSender(CreateServiceBusSender(sp)));

                    // Adds the CDC-hosted service(s) including orchestrator services, and specified EventOutbox dequeue/send service.
                    services.AddGeneratedCdcHostedServices()
                            .AddEventOutboxHostedService(sp => new EventOutboxDequeue(sp.GetRequiredService<IDatabase>(), new LoggerEventSender(sp.GetRequiredService<ILogger<LoggerEventSender>>()), sp.GetRequiredService<ILogger<EventOutboxDequeue>>()))
                            //.AddEventOutboxHostedService(sp => new EventOutboxDequeue(sp.GetService<IDatabase>(), CreateServiceBusSender(sp), sp.GetService<ILogger<EventOutboxDequeue>>()))
                            .AddGeneratedOrchestratorServices()
                            .AddFileLockSynchronizer();
                });

        /// <summary>
        /// Creates the ServiceBusSender using the specified default queue or topic name.
        /// </summary>
        private static CoreEx.Azure.ServiceBus.ServiceBusSender CreateServiceBusSender(System.IServiceProvider sp) => new CoreEx.Azure.ServiceBus.ServiceBusSender(
            sp.GetRequiredService<Azure.Messaging.ServiceBus.ServiceBusClient>(), sp.GetRequiredService<CoreEx.ExecutionContext>(), sp.GetRequiredService<CoreEx.Configuration.SettingsBase>(), sp.GetRequiredService<ILogger<CoreEx.Azure.ServiceBus.ServiceBusSender>>())
                { DefaultQueueOrTopicName = "AppName" };
    }
}