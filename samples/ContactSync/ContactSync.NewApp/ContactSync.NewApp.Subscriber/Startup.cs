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
            .AddExecutionContext()
            .AddJsonSerializer()
            .AddCloudEventSerializer()
            .AddEventSubscribers<Startup>()
            .AddEventSubscriberOrchestrator((_, o) =>
            {
                o.NotSubscribedHandling = ErrorHandling.CompleteAsSilent;
                o.AddSubscribers(EventSubscriberOrchestrator.GetSubscribers<Startup>());
            })
            .AddAzureServiceBusOrchestratedSubscriber((_, o) =>
            {
                o.EventDataDeserializationErrorHandling = ErrorHandling.ThrowSubscriberException;
                o.InvalidDataHandling = ErrorHandling.CompleteWithError;
            });
    }
}