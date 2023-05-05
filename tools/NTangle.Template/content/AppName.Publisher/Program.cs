namespace AppName.Publisher;

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
        .ConfigureHostConfiguration(c => c.AddEnvironmentVariables(prefix: "AppName_"))
        .ConfigureServices((services) =>
        {
            // Adds the required _CoreEx_ services.
            services.AddSettings<AppNameSettings>()
                    .AddLogging(b => b.AddSimpleConsole())
                    .AddDatabase(sp => new SqlServerDatabase(() => new SqlConnection(sp.GetRequiredService<AppNameSettings>().DatabaseConnectionString)))
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

            // Adds the ServiceBusSender to publish the events to Azure Service Bus, and starts the event outbox dequeue hosted service.
            //services.AddSingleton(sp => new Az.ServiceBusClient(sp.GetRequiredService<AppNameSettings>().ServiceBusConnectionString)))
            //        .AddScoped<ServiceBusSender>()
            //        .AddSqlServerEventOutboxHostedService(sp => new EventOutboxDequeue(sp.GetRequiredService<IDatabase>(), sp.GetRequiredService<ServiceBusSender>(), sp.GetRequiredService<ILogger<EventOutboxDequeue>>()));
        })
        .Build().Run();
}