/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using CoreEx.Configuration;
using CoreEx.Database;
using CoreEx.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NTangle;
using NTangle.Data;
using NTangle.Events;
using SqlServerDemo.Publisher.Data;
using SqlServerDemo.Publisher.Services;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides <see cref="IServiceCollection"/> extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the generated <see cref="NTangle.Cdc.IEntityOrchestrator"/> scoped services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddGeneratedOrchestratorServices(this IServiceCollection services)
            => services.AddScoped<IPostCdcOrchestrator, PostCdcOrchestrator>()
                       .AddScoped<IContactCdcOrchestrator, ContactCdcOrchestrator>()
                       .AddScoped<ICustomerCdcOrchestrator, CustomerCdcOrchestrator>();

        /// <summary>
        /// Adds the generated <see cref="NTangle.Services.CdcHostedService"/> services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddGeneratedCdcHostedServices(this IServiceCollection services)
            => services.AddCdcHostedService<PostHostedService>()
                       .AddCdcHostedService<ContactHostedService>()
                       .AddCdcHostedService<CustomerHostedService>();

        /// <summary>
        /// Adds the generated <see cref="EventOutboxEnqueue"/> as a <see cref="CoreEx.Events.IEventSender"/> scoped service.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configure">The action to enable the <see cref="EventOutboxEnqueue"/> to be further configured.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddGeneratedEventOutboxSender(this IServiceCollection services, Action<IServiceProvider, EventOutboxEnqueue>? configure = null)
            => services.AddScoped<IEventSender>(sp =>
            {
                var eoe = new EventOutboxEnqueue(sp.GetRequiredService<IDatabase>(), sp.GetRequiredService<ILogger<EventOutboxEnqueue>>());
                configure?.Invoke(sp, eoe);
                return eoe;
            });
    }
}

#pragma warning restore
#nullable restore