#if (implement_dbprovider_sqlserver)
using Microsoft.Data.SqlClient;
#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NTangle;
using NTangle.Data;
using NTangle.Events;

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
                    services.AddLogging(configure => configure.AddConsole())
#if (implement_dbprovider_sqlserver)
                            .AddDatabase(sp => new SqlServerDatabase(() => new SqlConnection(hostContext.Configuration.GetConnectionString("SqlDb"))))
#endif
                            .AddCloudEventSerializer()
                            .AddFileLockSynchronizer()
                            .AddGeneratedOutboxEventPublishing()
                            .AddGeneratedOrchestratorServices()
                            .AddGeneratedHostedServices(hostContext.Configuration)
                            .AddGeneratedOutboxDequeueHostedService<LoggerEventPublisher>(hostContext.Configuration);
                });
    }
}