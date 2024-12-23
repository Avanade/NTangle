﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Database.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using NTangle.Data;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class DatabaseServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an <see cref="ISidecarDatabase"/> as a scoped service.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="create">The function to create the <see cref="ISidecarDatabase"/> instance.</param>
        /// <param name="healthCheck">Indicates whether a corresponding <see cref="DatabaseHealthCheck{TDatabase}"/> should be configured.</param>
        /// <returns>The <see cref="IServiceCollection"/> to support fluent-style method-chaining.</returns>
        public static IServiceCollection AddSidecarDatabase(this IServiceCollection services, Func<IServiceProvider, ISidecarDatabase> create, bool healthCheck = true)
        {
            services.AddScoped(sp => create(sp) ?? throw new InvalidOperationException($"An {nameof(ISidecarDatabase)} instance must be instantiated."));
            return AddHealthCheck(services, healthCheck, null);
        }

        /// <summary>
        /// Adds an <see cref="ISidecarDatabase"/> as a scoped service including a corresponding health check.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="create">The function to create the <see cref="ISidecarDatabase"/> instance.</param>
        /// <param name="healthCheckName">The health check name; defaults to '<c>sidecar-db</c>'.</param>
        /// <returns>The <see cref="IServiceCollection"/> to support fluent-style method-chaining.</returns>
        public static IServiceCollection AddSidecarDatabase(this IServiceCollection services, Func<IServiceProvider, ISidecarDatabase> create, string? healthCheckName)
        {
            services.AddScoped(sp => create(sp) ?? throw new InvalidOperationException($"An {nameof(ISidecarDatabase)} instance must be instantiated."));
            return AddHealthCheck(services, true, healthCheckName);
        }

        /// <summary>
        /// Adds an <see cref="ISidecarDatabase"/> as a scoped service.
        /// </summary>
        /// <typeparam name="TDb">The <see cref="ISidecarDatabase"/> <see cref="Type"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="healthCheck">Indicates whether a corresponding <see cref="DatabaseHealthCheck{TDatabase}"/> should be configured.</param>
        /// <returns>The <see cref="IServiceCollection"/> to support fluent-style method-chaining.</returns>
        public static IServiceCollection AddSidecarDatabase<TDb>(this IServiceCollection services, bool healthCheck = true) where TDb : class, ISidecarDatabase
        {
            services.AddScoped<ISidecarDatabase, TDb>();
            return AddHealthCheck(services, healthCheck, null);
        }

        /// <summary>
        /// Adds an <see cref="ISidecarDatabase"/> as a scoped service including a corresponding health check.
        /// </summary>
        /// <typeparam name="TDb">The <see cref="ISidecarDatabase"/> <see cref="Type"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="healthCheckName">The health check name; defaults to '<c>database</c>'.</param>
        /// <returns>The <see cref="IServiceCollection"/> to support fluent-style method-chaining.</returns>
        public static IServiceCollection AddSidecarDatabase<TDb>(this IServiceCollection services, string? healthCheckName) where TDb : class, ISidecarDatabase
        {
            services.AddScoped<ISidecarDatabase, TDb>();
            return AddHealthCheck(services, true, healthCheckName);
        }

        /// <summary>
        /// Adds the <see cref="DatabaseHealthCheck{TDatabase}"/> where configured to do so.
        /// </summary>
        private static IServiceCollection AddHealthCheck(this IServiceCollection services, bool healthCheck, string? healthCheckName)
        {
            if (healthCheck)
                services.AddHealthChecks().AddTypeActivatedCheck<DatabaseHealthCheck<ISidecarDatabase>>(healthCheckName ?? "sidecar-db", HealthStatus.Unhealthy, tags: default!, timeout: TimeSpan.FromSeconds(30));

            return services;
        }
    }
}
