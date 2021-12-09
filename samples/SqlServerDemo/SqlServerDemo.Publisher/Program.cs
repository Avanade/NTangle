using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NTangle;
using NTangle.Data.SqlServer;
using NTangle.Events;

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
                    services.AddLogging(configure => configure.AddConsole())
                            .AddDatabase(sp => new SqlServerDatabase(() => new SqlConnection(hostContext.Configuration.GetConnectionString("SqlDb"))))
                            .AddStringIdentifierGenerator()
                            .AddCloudEventSerializer()
                            .AddFileLockSynchronizer()
                            .AddGeneratedOutboxEventPublishing()
                            .AddGeneratedOrchestratorServices()
                            .AddGeneratedHostedServices(hostContext.Configuration)
                            .AddOutboxDequeueHostedService<LoggerEventPublisher>(hostContext.Configuration);
                });
    }
}