// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Configuration;
using CoreEx.Hosting;
using Microsoft.Extensions.Logging;
using NTangle.Cdc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NTangle.Services
{
    /// <summary>
    /// Provides the base Change Data Capture (CDC) self-orchestrated <see cref="ServiceBase"/> capabilities.
    /// </summary>
    /// <remarks>Note that no <see cref="IServiceSynchronizer"/> is leveraged; synchronization is the responsibility of the consumer.</remarks>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    /// <param name="settings">The <see cref="SettingsBase"/>.</param>
    public abstract class CdcService<TOrchestrator, TEntity>(IServiceProvider serviceProvider, ILogger logger, SettingsBase? settings) : ServiceBase(serviceProvider, logger, settings) where TOrchestrator : notnull, IEntityOrchestrator<TEntity> where TEntity : IEntity
    {
        private int? _maxIterations = null;

        /// <summary>
        /// Gets or sets the maximum number of iterations per execution.
        /// </summary>
        public override int MaxIterations
        {
            get => _maxIterations ?? Settings.GetCdcValue<int?>(ServiceName, MaxIterationsName) ?? DefaultMaxIterations;
            set => _maxIterations = value <= 0 ? DefaultMaxIterations : value;
        }

        /// <summary>
        /// Execute the <typeparamref name="TOrchestrator"/>.
        /// </summary>
        /// <param name="scopedServiceProvider">The scoped <see cref="IServiceProvider"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns><c>true</c> indicates to execute the next iteration (i.e. continue); otherwise, <c>false</c> to stop.</returns>
        protected override async Task<bool> ExecuteAsync(IServiceProvider scopedServiceProvider, CancellationToken cancellationToken)
        {
            // Instantiate the orchestrator.
            var eo = (TOrchestrator)(scopedServiceProvider.GetService(typeof(TOrchestrator))
                ?? throw new InvalidOperationException($"Attempted to get service '{typeof(TOrchestrator).FullName}' but null was returned; this would indicate that the service has not been configured correctly."));

            // Execute the orchestrator.
            var result = await eo.ExecuteAsync(cancellationToken).ConfigureAwait(false);

            // Where successful and a batch was processed, then the next batch should be attempted immediately; otherwise, retry later.
            return result.IsSuccessful && result.BatchTracker != null;
        }
    }
}