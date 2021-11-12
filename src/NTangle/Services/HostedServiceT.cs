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
    /// <typeparam name="TOrchestrator">The <see cref="IEntityOrchestrator"/> <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntity">The underlying <see cref="IEntity"/> <see cref="Type"/>.</typeparam>
    public abstract class HostedService<TOrchestrator, TEntity> : HostedService where TOrchestrator : IEntityOrchestrator<TEntity> where TEntity : IEntity
    {
        private readonly IHostedServiceSynchronizer _synchronizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostedService{T, TEntity}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The owning <see cref="IServiceProvider"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <param name="synchronizer"> The <see cref="IHostedServiceSynchronizer"/>.</param>
        public HostedService(IServiceProvider serviceProvider, ILogger logger, IConfiguration config, IHostedServiceSynchronizer synchronizer) : base(serviceProvider, logger, config) 
            => _synchronizer = synchronizer ?? throw new ArgumentNullException(nameof(synchronizer));

        /// <summary>
        /// Executes the entity orchestration (<see cref="IEntityOrchestrator.ExecuteAsync(CancellationToken?)"/>) for the next batch and/or last incomplete batch.
        /// </summary>
        /// <param name="scopedServiceProvider"><inheritdoc/></param>
        /// <param name="cancellationToken"><inheritdoc/></param>
        protected override async Task ExecuteAsync(IServiceProvider scopedServiceProvider, CancellationToken cancellationToken)
        {
            // Ensure we have synchronized control; if not exit immediately.
            if (!_synchronizer.Enter<TEntity>())
                return;

            try
            {
                // Instantiate and initialize the orchestrator.
                var eo = (TOrchestrator)scopedServiceProvider.GetService(typeof(TOrchestrator)) ?? throw new InvalidOperationException($"Attempted to get service '{typeof(TOrchestrator).FullName}' but null was returned; this would indicate that the service has not been configured correctly.");

                var mqs = MaxQuerySize;
                if (mqs.HasValue)
                    eo.MaxQuerySize = mqs.Value;

                var cwdl = ContinueWithDataLoss;
                if (cwdl.HasValue)
                    eo.ContinueWithDataLoss = cwdl.Value;

                // Keep executing until unsuccessful or reached end of current CDC data stream.
                while (true)
                {
                    var result = await eo.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    // Where successful and a batch was processed, then the next batch should be attempted immediately; otherwise, retry later.
                    if (!result.IsSuccessful || result.Batch == null)
                        return;
                }
            }
            finally
            {
                _synchronizer.Exit<TEntity>();
            }
        }
    }
}