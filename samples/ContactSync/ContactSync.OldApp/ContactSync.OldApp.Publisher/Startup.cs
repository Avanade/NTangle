[assembly: FunctionsStartup(typeof(ContactSync.OldApp.Publisher.Startup))]

namespace ContactSync.OldApp.Publisher;

/// <summary>
/// The console program leveraging <see href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host"/>.
/// </summary>
public class Startup : FunctionsStartup
{
    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder) => builder.ConfigurationBuilder
        .AddJsonFile(System.IO.Path.Combine(builder.GetContext().ApplicationRootPath ?? "", "appsettings.json"), optional: true)
        .AddEnvironmentVariables("OldApp_");

    public override void Configure(IFunctionsHostBuilder builder)
    {
        // Adds the required _CoreEx_ services.
        builder.Services
            .AddSettings<OldAppSettings>()
            .AddDatabase(sp => new SqlServerDatabase(() => new SqlConnection(sp.GetRequiredService<OldAppSettings>().DatabaseConnectionString)))
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
            .AddSingleton(sp => new Az.ServiceBusClient(sp.GetRequiredService<OldAppSettings>().ServiceBusConnectionString))
            .AddScoped<ServiceBusSender>()
            .AddScoped(sp => new EventOutboxService(sp, sp.GetRequiredService<ILogger<EventOutboxService>>())
            {
                EventOutboxDequeueFactory = sp => new EventOutboxDequeue(sp.GetRequiredService<IDatabase>(), sp.GetRequiredService<ServiceBusSender>(), sp.GetRequiredService<ILogger<EventOutboxDequeue>>())
            });
    }
}