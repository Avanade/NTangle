// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NTangle.Cdc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NTangle.Services
{
    /// <summary>
    /// Provides the base Change Data Capture (CDC) <see cref="TimerHostedServiceBase"/> capabilities for a specified <see cref="IEntityOrchestrator"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="IEntityOrchestrator"/> <see cref="Type"/>.</typeparam>
    public abstract class HostedService<T> : HostedService where T : IEntityOrchestrator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HostedService{T}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The owning <see cref="IServiceProvider"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        public HostedService(IServiceProvider serviceProvider, ILogger logger, IConfiguration? config = null) : base(serviceProvider, logger, config) { }

        /// <summary>
        /// Executes the entity orchestration (<see cref="IEntityOrchestrator.ExecuteAsync(CancellationToken?)"/>) for the next batch and/or last incomplete batch.
        /// </summary>
        /// <param name="scopedServiceProvider"><inheritdoc/></param>
        /// <param name="cancellationToken"><inheritdoc/></param>
        protected override async Task ExecuteAsync(IServiceProvider scopedServiceProvider, CancellationToken cancellationToken)
        {
            var eo = (T)scopedServiceProvider.GetService(typeof(T)) ?? throw new InvalidOperationException($"Attempted to get service '{typeof(T).FullName}' but null was returned; this would indicate that the service has not been configured correctly.");

            var mqs = MaxQuerySize;
            if (mqs.HasValue)
                eo.MaxQuerySize = mqs.Value;

            var cwdl = ContinueWithDataLoss;
            if (cwdl.HasValue)
                eo.ContinueWithDataLoss = cwdl.Value;

            // Keep executing until unsuccessful or reached end of CDC data.
            while (true)
            {
                var result = await eo.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                if (cancellationToken.IsCancellationRequested)
                    return;

                // Where successful and a batch was processed, then the next batch should be attempted immediately.
                if (!result.IsSuccessful || result.Batch == null)
                    return; // Retry later.
            }
        }
    }
}