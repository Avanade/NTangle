// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Configuration;
using CoreEx.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NTangle.Cdc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NTangle.Services
{
    /// <summary>
    /// Provides the base Change Data Capture (CDC) <see cref="TimerHostedServiceBase"/> capabilities.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    /// <param name="settings">The <see cref="SettingsBase"/>.</param>
    /// <param name="synchronizer">The <see cref="IServiceSynchronizer"/>.</param>
    public abstract class CdcHostedService<TOrchestrator, TEntity>(IServiceProvider serviceProvider, ILogger logger, SettingsBase? settings, IServiceSynchronizer? synchronizer) : SynchronizedTimerHostedServiceBase<TEntity>(serviceProvider, logger, settings, synchronizer), ICdcHostedService where TOrchestrator : notnull, IEntityOrchestrator<TEntity> where TEntity : IEntity
    {
        private TimeSpan? _interval;

        /// <summary>
        /// The configuration settings name for <see cref="Interval"/>.
        /// </summary>
        public const string IntervalName = "Interval";

        /// <summary>
        /// Gets or sets the default interval seconds used where the specified <see cref="Interval"/> is less than or equal to zero. Defaults to <b>thirty</b> seconds.
        /// </summary>
        public static TimeSpan DefaultInterval { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets or sets the interval between each execution.
        /// </summary>
        public override TimeSpan Interval
        {
            get => _interval ?? Settings.GetCdcValue<TimeSpan?>(ServiceName, IntervalName) ?? DefaultInterval;
            set => _interval = value;
        }

        /// <inheritdoc/>
        protected override async Task SynchronizedExecuteAsync(IServiceProvider scopedServiceProvider, CancellationToken cancellationToken)
        {
            // Keep executing until unsuccessful or reached end of current CDC data stream.
            while (true)
            {
                // New scope per iteration to ensure any dependencies are disposed correctly; reset the execution context between invocations.
                using var scope = scopedServiceProvider.CreateScope();
                using var ec = CoreEx.ExecutionContext.CreateNew();
                ec.ServiceProvider = scope.ServiceProvider;

                // Instantiate the orchestrator.
                var eo = (TOrchestrator)(scope.ServiceProvider.GetService(typeof(TOrchestrator))
                    ?? throw new InvalidOperationException($"Attempted to get service '{typeof(TOrchestrator).FullName}' but null was returned; this would indicate that the service has not been configured correctly."));

                // Execute the orchestrator.
                var result = await eo.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                if (cancellationToken.IsCancellationRequested)
                    return;

                // Where successful and a batch was processed, then the next batch should be attempted immediately; otherwise, retry later.
                if (!result.IsSuccessful || result.BatchTracker == null)
                    return;
            }
        }
    }
}