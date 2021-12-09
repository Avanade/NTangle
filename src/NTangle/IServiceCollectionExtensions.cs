// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using DbEx;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NTangle.Events;
using NTangle.Services;
using System;
using System.Diagnostics;
using System.Linq;

namespace NTangle
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
        /// Adds the <typeparamref name="T"/> <see cref="HostedService"/> using the <see cref="ServiceCollectionHostedServiceExtensions.AddHostedService{THostedService}(IServiceCollection)"/>. 
        /// </summary>
        /// <typeparam name="T">The <see cref="HostedService"/> <see cref="Type"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <remarks>Before adding checks whether the <typeparamref name="T"/> has been specified within the comma-separated list of selected services defined in the <paramref name="config"/> using the <see cref="ServicesKey"/> to confirm.
        /// Where no selected services have been specified within the <paramref name="config"/> then the service will always be added.</remarks>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddNTangleHostedService<T>(this IServiceCollection services, IConfiguration config) where T : HostedService
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var svcs = (config ?? throw new ArgumentNullException(nameof(config))).GetValue<string?>(ServicesKey);
            if (string.IsNullOrEmpty(svcs))
            {
                services.AddHostedService<T>();
                return services;
            }

            var svcsList = svcs.Split(",", StringSplitOptions.RemoveEmptyEntries);
            if (svcsList.Contains(typeof(T).Name, StringComparer.OrdinalIgnoreCase) || (typeof(T).Name.EndsWith(HostedServiceSuffix) && svcsList.Contains(typeof(T).Name[..^HostedServiceSuffix.Length], StringComparer.OrdinalIgnoreCase)))
                services.AddHostedService<T>();

            return services;
        }

        /// <summary>
        /// Adds the <see cref="IDatabase"/> scoped service.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="create">The function to create the <see cref="IDatabase"/> instance.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddDatabase(this IServiceCollection services, Func<IServiceProvider, IDatabase> create) => CheckServices(services).AddScoped(create ?? throw new ArgumentNullException(nameof(create)));

        /// <summary>
        /// Adds the <see cref="IdentifierGenerator"/> as the <see cref="string"/> <see cref="IIdentifierGenerator{T}"/> singleton service.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddStringIdentifierGenerator(this IServiceCollection services) => CheckServices(services).AddSingleton<IIdentifierGenerator<string>, IdentifierGenerator>();

        /// <summary>
        /// Adds the <see cref="IdentifierGenerator"/> as the <see cref="Guid"/> <see cref="IIdentifierGenerator{T}"/> singleton service.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddGuidIdentifierGenerator(this IServiceCollection services) => CheckServices(services).AddSingleton<IIdentifierGenerator<Guid>, IdentifierGenerator>();

        /// <summary>
        /// Adds the <see cref="CloudEventSerializer"/> as the <see cref="IEventSerializer"/> scoped service.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddCloudEventSerializer(this IServiceCollection services) => CheckServices(services).AddScoped<IEventSerializer, CloudEventSerializer>();

        /// <summary>
        /// Adds the <see cref="LoggerEventPublisher"/> as the <see cref="IEventPublisher"/> scoped service.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddLoggerEventPublisher(this IServiceCollection services) => CheckServices(services).AddScoped<IEventPublisher, LoggerEventPublisher>();

        /// <summary>
        /// Adds the <see cref="FileLockSynchronizer"/> as the <see cref="IServiceSynchronizer"/> scoped service.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddFileLockSynchronizer(this IServiceCollection services) => CheckServices(services).AddScoped<IServiceSynchronizer, FileLockSynchronizer>();

        /// <summary>
        /// Adds the <see cref="OutboxDequeueHostedService"/>(s) (see <see cref="HostedService"/>) whilst also adding the corresponding <typeparamref name="TPublisher"/> as the <see cref="IOutboxEventPublisher"/> scoped service.
        /// </summary>
        /// <typeparam name="TPublisher">The <see cref="IOutboxEventPublisher"/> <see cref="Type"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <param name="partitionKeys">The optional list of partition keys; one <see cref="OutboxDequeueHostedService"/> instance will be started per specified key.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        /// <remarks>To turn off the execution of the hosted service at runtime set the '<c>OutboxDequeue</c>' configuration setting to <c>false</c>.</remarks>
        public static IServiceCollection AddOutboxDequeueHostedService<TPublisher>(this IServiceCollection services, IConfiguration config, params string?[] partitionKeys) where TPublisher : class, IOutboxEventPublisher
        {
            var exe = (config ?? throw new System.ArgumentNullException(nameof(config))).GetValue<bool?>("OutboxDequeue");
            if (!exe.HasValue || exe.Value)
            {
                services.AddScoped<IOutboxEventPublisher, TPublisher>();
                if (partitionKeys == null || partitionKeys.Length == 0)
                    partitionKeys = new string?[] { null };

                foreach (var pk in partitionKeys.Distinct())
                {
                    services.AddHostedService(sp => new OutboxDequeueHostedService(sp, sp.GetRequiredService<ILogger<OutboxDequeueHostedService>>(), config, sp.GetRequiredService<IServiceSynchronizer>(), pk));
                }
            }

            return services;
        }
    }
}