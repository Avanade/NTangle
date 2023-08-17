#if (implement_publisher_function)
[assembly: FunctionsStartup(typeof(AppName.Publisher.Startup))]
#endif

namespace AppName.Publisher;

#if (implement_publisher_function)
/// <summary>
/// The Azure Functions runtime startup leveraging <see href="https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection"/>.
/// </summary>
public class Startup : FunctionsStartup
{
    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder) => builder.ConfigurationBuilder
        .AddJsonFile(System.IO.Path.Combine(builder.GetContext().ApplicationRootPath ?? "", "appsettings.json"), optional: true)
        .AddEnvironmentVariables("DomainName_");

    public override void Configure(IFunctionsHostBuilder builder)
    {
        // Adds the required _CoreEx_ services.
        builder.Services
            .AddSettings<DomainNameSettings>()
            .AddDatabase(sp => new SqlServerDatabase(() => new SqlConnection(sp.GetRequiredService<DomainNameSettings>().DatabaseConnectionString)))
            .AddExecutionContext()
            .AddJsonSerializer();

        // Adds underlying CDC orchestrator/services.
        builder.Services
            .AddGeneratedCdcOrchestratorServices()
            .AddGeneratedCdcServices();

        // Adds the EventPublisher, which will use the default EventDataFormatter, with CloudEventSerializer and EventOutbox enqueue as sender.
        builder.Services
            .AddEventPublisher()
            .AddEventDataFormatter()
            .AddCloudEventSerializer()
            .AddScoped<IEventSender, EventOutboxEnqueue>();

        //  Adds the ServiceBusSender to publish the events to Azure Service Bus, and adds the event outbox dequeue service.
        builder.Services
            .AddSingleton(sp => new Az.ServiceBusClient(sp.GetRequiredService<DomainNameSettings>().ServiceBusConnectionString))
            .AddScoped<ServiceBusSender>()
            .AddScoped(sp => new EventOutboxService(sp, sp.GetRequiredService<ILogger<EventOutboxService>>())
            {
                EventOutboxDequeueFactory = sp => new EventOutboxDequeue(sp.GetRequiredService<IDatabase>(), sp.GetRequiredService<ServiceBusSender>(), sp.GetRequiredService<ILogger<EventOutboxDequeue>>())
            });
    }
}
#else
/// <summary>
/// The <see cref="HostStartup"/> to enable testable dependency injection.
/// </summary>
public class Startup : HostStartup
{
    public override void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder config)
    {
        config.AddEnvironmentVariables(prefix: "DomainName_");
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        // Adds the required _CoreEx_ services.
        services.AddSettings<DomainNameSettings>()
                .AddLogging(b => b.AddSimpleConsole())
                .AddDatabase(sp => new SqlServerDatabase(() => new SqlConnection(sp.GetRequiredService<DomainNameSettings>().DatabaseConnectionString)))
                .AddStringIdentifierGenerator()
                .AddExecutionContext()
                .AddJsonSerializer();

        // Adds the CDC-orchestrator services and local file synchronizer (to run-as singleton).
        services.AddGeneratedCdcOrchestratorServices()
                .AddFileLockSynchronizer();

        // Adds the EventPublisher, which will use the default EventDataFormatter, with CloudEventSerializer and EventOutbox enqueue as sender.
        services.AddEventPublisher()
                .AddEventDataFormatter()
                .AddCloudEventSerializer()
                .AddScoped<IEventSender, EventOutboxEnqueue>();

        // For demo/testing purposes uses the LoggerEventSender; remove usage and uncomment Azure Service Bus below.
        services.AddScoped<LoggerEventSender>();

        // Adds the ServiceBusSender to publish the events to Azure Service Bus, and starts the event outbox dequeue (relay) hosted service.
        //services.AddSingleton(sp => new Az.ServiceBusClient(sp.GetRequiredService<DomainNameSettings>().ServiceBusConnectionString))
        //        .AddScoped<ServiceBusSender>();
    }
}
#endif