[assembly: FunctionsStartup(typeof(ContactSync.NewApp.Subscriber.Startup))]

namespace ContactSync.NewApp.Subscriber;

/// <summary>
/// The console program leveraging <see href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host"/>.
/// </summary>
public class Startup : FunctionsStartup
{
    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder) => builder.ConfigurationBuilder
        .AddJsonFile(System.IO.Path.Combine(builder.GetContext().ApplicationRootPath ?? "", "appsettings.json"), optional: true)
        .AddEnvironmentVariables("NewApp_");

    public override void Configure(IFunctionsHostBuilder builder)
    {
        // Adds the required _CoreEx_ services.
        builder.Services
            .AddSettings<NewAppSettings>()
            .AddDatabase(sp => new SqlServerDatabase(() => new SqlConnection(sp.GetRequiredService<NewAppSettings>().DatabaseConnectionString)))
            .AddExecutionContext()                                                          // Registers the execution context (AsyncLocal).
            .AddJsonSerializer()                                                            // Registers the System.Text.Json serializer/deserializer.
            .AddCloudEventSerializer()                                                      // Registers the CloudEvent serializer/deserializer.
            .AddEventSubscribers<Startup>()                                                 // Registers the event subscribers from the specified assembly.
            .AddEventSubscriberOrchestrator((_, o) =>                                       // Registers the event subscriber orchestrator
            {
                o.NotSubscribedHandling = ErrorHandling.CompleteAsSilent;                   // Where event is not subscribed, complete the message without error/warn/info; i.e. silently.
                o.AddSubscribers(EventSubscriberOrchestrator.GetSubscribers<Startup>());    // Links the subscribers from the specified assembly using EventSubscriberAttribute.
            })
            .AddAzureServiceBusOrchestratedSubscriber((_, o) =>                             // Register the AzureServiceBusOrchestratedSubscriber (uses EventSubscriberOrchestrator above); this is what is used in the Azure Function.
            {
                o.EventDataDeserializationErrorHandling = ErrorHandling.Handle;
                o.InvalidDataHandling = ErrorHandling.CompleteWithError;
            });
    }
}