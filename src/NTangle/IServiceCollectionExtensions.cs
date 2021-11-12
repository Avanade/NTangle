// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    }
}