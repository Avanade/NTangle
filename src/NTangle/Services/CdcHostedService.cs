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
    /// Provides the base Change Data Capture (CDC) <see cref="TimerHostedServiceBase"/> capabilities.
    /// </summary>
    public abstract class CdcHostedService<TOrchestrator, TEntity> : SynchronizedTimerHostedServiceBase<TEntity>, ICdcHostedService where TOrchestrator : IEntityOrchestrator<TEntity> where TEntity : IEntity
    {
        private TimeSpan? _interval;
        private int? _maxQuerySize;
        private bool? _continueWithDataLoss;

        /// <summary>
        /// The configuration settings name for <see cref="Interval"/>.
        /// </summary>
        public const string IntervalName = "Interval";

        /// <summary>
        /// The configuration settings name for <see cref="MaxQuerySize"/>.
        /// </summary>
        public const string MaxQuerySizeName = "MaxQuerySize";

        /// <summary>
        /// The configuration settings name for <see cref="ContinueWithDataLoss"/>.
        /// </summary>
        public const string ContinueWithDataLossName = "ContinueWithDataLoss";

        /// <summary>
        /// Gets or sets the default interval seconds used where the specified <see cref="Interval"/> is less than or equal to zero. Defaults to <b>thirty</b> seconds.
        /// </summary>
        public static TimeSpan DefaultInterval { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Initializes a new instance of the <see cref="CdcHostedService{TOrchestrator, TEntity}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="settings">The <see cref="SettingsBase"/>.</param>
        /// <param name="synchronizer">The <see cref="IServiceSynchronizer"/>.</param>
        protected CdcHostedService(IServiceProvider serviceProvider, ILogger logger, SettingsBase? settings, IServiceSynchronizer? synchronizer) : base(serviceProvider, logger, settings, synchronizer) { }

        /// <summary>
        /// Gets or sets the interval between each execution.
        /// </summary>
        /// <remarks>Will default to configuration, a) <see cref="TimerHostedServiceBase.ServiceName"/>_<see cref="IntervalName"/> (e.g. '<c>ContactService_Interval</c>'), then b) <see cref="IntervalName"/>, where specified; otherwise, <see cref="DefaultInterval"/>.</remarks>
        public override TimeSpan Interval
        {
            get => _interval ?? Settings.GetValue<TimeSpan?>($"{ServiceName}_{IntervalName}") ?? Settings.GetValue<TimeSpan?>(IntervalName) ?? DefaultInterval;
            set => _interval = value;
        }

        /// <summary>
        /// Gets or sets the maximum query size to limit the number of CDC (Change Data Capture) rows that are batched in a <see cref="BatchTracker"/>.
        /// </summary>
        /// <remarks>Where specified overrides the <see cref="IEntityOrchestrator.MaxQuerySize"/>.
        /// <para>Will default to configuration, a) <see cref="TimerHostedServiceBase.ServiceName"/>_<see cref="MaxQuerySizeName"/> (e.g. '<c>ContactService_MaxQuerySize</c>'), then b) <see cref="MaxQuerySizeName"/>, where specified.</para></remarks>
        public virtual int? MaxQuerySize
        {
            get => _maxQuerySize ?? Settings.GetValue<int?>($"{ServiceName}_{MaxQuerySizeName}") ?? Settings.GetValue<int?>(MaxQuerySizeName);
            set => _maxQuerySize = value;
        }

        /// <summary>
        /// Indicates whether to ignore any data loss and continue using the CDC (Change Data Capture) data that is available.
        /// </summary>
        /// <remarks>For more information as to why data loss may occur for SQL Server see: https://docs.microsoft.com/en-us/sql/relational-databases/track-changes/administer-and-monitor-change-data-capture-sql-server 
        /// <para>Will default to configuration settings, a) <see cref="TimerHostedServiceBase.ServiceName"/>_<see cref="ContinueWithDataLossName"/> (e.g. '<c>ContactService_ContinueWithDataLoss</c>'), then b) <see cref="ContinueWithDataLossName"/>, where specified.</para></remarks>
        public virtual bool? ContinueWithDataLoss
        {
            get => _continueWithDataLoss ?? Settings.GetValue<bool?>($"{ServiceName}_{ContinueWithDataLossName}") ?? Settings.GetValue<bool?>(ContinueWithDataLossName);
            set => _continueWithDataLoss = value;
        }

        /// <inheritdoc/>
        protected override async Task SynchronizedExecuteAsync(IServiceProvider scopedServiceProvider, CancellationToken cancellationToken)
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
    }
}