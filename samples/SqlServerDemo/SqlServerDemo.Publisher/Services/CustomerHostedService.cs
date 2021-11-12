/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NTangle.Services;
using System;
using SqlServerDemo.Publisher.Data;
using SqlServerDemo.Publisher.Entities;

namespace SqlServerDemo.Publisher.Services
{
    /// <summary>
    /// Provides the Change Data Capture (CDC) <see cref="CustomerCdc"/> entity (aggregate root) <see cref="HostedService{T}"/> capabilities (database table '[Legacy].[Customer]').
    /// </summary>
    public partial class CustomerHostedService : HostedService<ICustomerCdcOrchestrator, CustomerCdc>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerHostedService"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="synchronizer"> The <see cref="IHostedServiceSynchronizer"/>.</param>
        public CustomerHostedService(IServiceProvider serviceProvider, ILogger<CustomerHostedService> logger, IConfiguration config, IHostedServiceSynchronizer synchronizer) : base(serviceProvider, logger, config, synchronizer) { }
    }
}

#pragma warning restore
#nullable restore