// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Configuration;
using CoreEx.Hosting;
using CoreEx.Database;
using CoreEx.Database.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NTangle.Services;
using System;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Adds <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    [DebuggerStepThrough]
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Gets the services <see cref="IConfiguration"/> key.
        /// </summary>
        public const string ServicesKey = "Services";

        /// <summary>
        /// Gets the <see cref="TimerHostedServiceBase.ServiceName"/> suffix as per the standard (code-generation) naming convention.
        /// </summary>
        public const string HostedServiceSuffix = "HostedService";

        /// <summary>
        /// Gets the outbox service <see cref="IConfiguration"/> key.
        /// </summary>
        public const string OutboxServiceKey = "OutboxService";

        /// <summary>
        /// Checks that the <see cref="IServiceCollection"/> is not null.
        /// </summary>
        private static IServiceCollection CheckServices(IServiceCollection services) => services ?? throw new ArgumentNullException(nameof(services));

        /// <summary>
        /// Adds the <typeparamref name="THostedService"/> <see cref="ICdcHostedService"/> using the <see cref="ServiceCollectionHostedServiceExtensions.AddHostedService{THostedService}(IServiceCollection)"/>. 
        /// </summary>
        /// <typeparam name="THostedService">The <see cref="ICdcHostedService"/> <see cref="Type"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <remarks>Before adding, checks whether the <typeparamref name="THostedService"/> has been specified within the comma-separated list of selected services defined in <see cref="SettingsBase"/> using the <see cref="ServicesKey"/> to confirm.
        /// Where no selected services have been specified within the <see cref="SettingsBase"/> then the service will always be added.</remarks>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddCdcHostedService<THostedService>(this IServiceCollection services) where THostedService : class, ICdcHostedService
        {
            var svcs = CheckServices(services).BuildServiceProvider().GetRequiredService<SettingsBase>().GetValue<string?>(ServicesKey);
            if (string.IsNullOrEmpty(svcs))
            {
                services.AddHostedService<THostedService>();
                return services;
            }

            var svcsList = svcs.Split(",", StringSplitOptions.RemoveEmptyEntries);
            if (svcsList.Contains(typeof(THostedService).Name, StringComparer.OrdinalIgnoreCase) || (typeof(THostedService).Name.EndsWith(HostedServiceSuffix) && svcsList.Contains(typeof(THostedService).Name[..^HostedServiceSuffix.Length], StringComparer.OrdinalIgnoreCase)))
                services.AddHostedService<THostedService>();

            return services;
        }

        /// <summary>
        /// Adds the <see cref="EventOutboxHostedService"/> using the <see cref="ServiceCollectionHostedServiceExtensions.AddHostedService{THostedService}(IServiceCollection)"/>. 
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="eventOutboxDequeueFactory">The function to create an instance of <see cref="EventOutboxDequeueBase"/> (used to set the underlying <see cref="EventOutboxHostedService.EventOutboxDequeueFactory"/> property).</param>
        /// <param name="partitionKey">The optional partition key.</param>
        /// <param name="destination">The optional destination name (i.e. queue or topic).</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        /// <remarks>To turn off the execution of the <see cref="EventOutboxHostedService"/>(s) at runtime set the '<c>EventOutboxHostedService</c>' configuration setting to <c>false</c>.</remarks>
        public static IServiceCollection AddEventOutboxHostedService(this IServiceCollection services, Func<IServiceProvider, EventOutboxDequeueBase> eventOutboxDequeueFactory, string? partitionKey = null, string? destination = null)
        {
            var exe = CheckServices(services).BuildServiceProvider().GetRequiredService<SettingsBase>().GetValue<bool?>("EventOutboxHostedService");
            if (!exe.HasValue || exe.Value)
            {
                services.AddHostedService(sp => new EventOutboxHostedService(sp, sp.GetRequiredService<ILogger<EventOutboxHostedService>>(), sp.GetRequiredService<SettingsBase>(), sp.GetRequiredService<IServiceSynchronizer>(), partitionKey, destination)
                {
                    EventOutboxDequeueFactory = eventOutboxDequeueFactory ?? throw new ArgumentNullException(nameof(eventOutboxDequeueFactory))
                });
            }

            return services;
        }
    }
}