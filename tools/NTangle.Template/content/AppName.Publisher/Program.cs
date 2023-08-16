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
        .ConfigurHostStartup<Startup>()
        .ConfigureServices((_, services) =>
        {
            // Adds the CDC-triggered hosted service(s) and corresponding event outbox dequeue (relay) hosted service.
            services.AddGeneratedCdcHostedServices()
                    .AddSqlServerEventOutboxHostedService(sp => new EventOutboxDequeue(sp.GetRequiredService<IDatabase>(), sp.GetRequiredService<LoggerEventSender>(), sp.GetRequiredService<ILogger<EventOutboxDequeue>>()));
            //        .AddSqlServerEventOutboxHostedService(sp => new EventOutboxDequeue(sp.GetRequiredService<IDatabase>(), sp.GetRequiredService<ServiceBusSender>(), sp.GetRequiredService<ILogger<EventOutboxDequeue>>()));
        })
        .Build().Run();
}