namespace SqlServerDemo.Publisher;

/// <summary>
/// The console program leveraging <see href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host"/>.
/// </summary>
internal class Program
{
    /// <summary>
    /// Main entry point and host builder.
    /// </summary>
    /// <param name="args">The console arguments.</param>
    internal static void Main(string[] args) => Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(c => c.AddEnvironmentVariables(prefix: "SqlServerDemo_"))
        .ConfigureServices((services) =>
        {
            services.AddSettings<SqlServerDemoSettings>()
                    .AddLogging(b => b.AddSimpleConsole())
                    .AddDatabase(sp => new SqlServerDatabase(() => new SqlConnection(sp.GetRequiredService<SqlServerDemoSettings>().DatabaseConnectionString)))
                    .AddStringIdentifierGenerator()
                    .AddExecutionContext()
                    .AddJsonSerializer();

            // Adds the CDC-hosted service(s), including underlying orchestrator services, and local file synchronizer (to run-as singleton).
            services.AddGeneratedCdcHostedServices()
                    .AddGeneratedCdcOrchestratorServices()
                    .AddFileLockSynchronizer();

            // Adds the EventPublisher, which will use the default EventDataFormatter, with CloudEventSerializer and EventOutbox enqueue as sender.
            services.AddEventPublisher()
                    .AddEventDataFormatter()
                    .AddCloudEventSerializer()
                    .AddScoped<IEventSender, EventOutboxEnqueue>();

            // For demo/testing purposes uses the LoggerEventSender; remove usage and uncomment Azure Service Bus below.
            services.AddScoped<LoggerEventSender>()
                    .AddSqlServerEventOutboxHostedService(sp => new EventOutboxDequeue(sp.GetRequiredService<IDatabase>(), sp.GetRequiredService<LoggerEventSender>(), sp.GetRequiredService<ILogger<EventOutboxDequeue>>()));

            // Adds the ServiceBusSender to publish the events to Azure Service Bus, and starts the event outbox dequeue (relay) hosted service.
            //services.AddSingleton(sp => new Az.ServiceBusClient(sp.GetRequiredService<SqlServerDemoSettings>().ServiceBusConnectionString))
            //        .AddScoped<ServiceBusSender>()
            //        .AddSqlServerEventOutboxHostedService(sp => new EventOutboxDequeue(sp.GetRequiredService<IDatabase>(), sp.GetRequiredService<ServiceBusSender>(), sp.GetRequiredService<ILogger<EventOutboxDequeue>>()));
        })
        .Build().Run();
}