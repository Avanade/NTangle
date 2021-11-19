// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NTangle.Cdc;
using NTangle.Data;
using NTangle.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NTangle.Services
{
    /// <summary>
    /// Provides the base <see cref="EventOutbox"/> dequeue and publish <see cref="TimerHostedServiceBase"/> capabilities.
    /// </summary>
    /// <remarks>This will instantiate an <see cref="IOutboxDequeuePublisher"/> and invoke <see cref="IOutboxDequeuePublisher.DequeueAndPublishAsync(int, CancellationToken)"/>.</remarks>
    public sealed class OutboxDequeueHostedService : TimerHostedServiceBase
    {
        private readonly IServiceSynchronizer _synchronizer;
        private TimeSpan? _interval;
        private int? _maxQuerySize;

        /// <summary>
        /// The configuration name for <see cref="Interval"/>.
        /// </summary>
        public const string IntervalName = "OutboxInterval";

        /// <summary>
        /// The configuration name for <see cref="MaxDequeueSize"/>.
        /// </summary>
        public const string MaxDequeueSizeName = "OutboxMaxDequeueSize";

        /// <summary>
        /// Gets or sets the default interval seconds used where the specified <see cref="Interval"/> is less than or equal to zero. Defaults to <b>thirty</b> seconds.
        /// </summary>
        public static TimeSpan DefaultInterval { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Initializes a new instance of the <see cref="HostedService"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <param name="synchronizer">The <see cref="IServiceSynchronizer"/>.</param>
        public OutboxDequeueHostedService(IServiceProvider serviceProvider, ILogger<OutboxDequeueHostedService> logger, IConfiguration config, IServiceSynchronizer synchronizer) : base(serviceProvider, logger, config)
            => _synchronizer = synchronizer ?? throw new ArgumentNullException(nameof(synchronizer));

        /// <summary>
        /// Gets or sets the interval between each execution.
        /// </summary>
        /// <remarks>Will default to configuration, a) <see cref="TimerHostedServiceBase.ServiceName"/> : <see cref="IntervalName"/>, then b) <see cref="IntervalName"/>, where specified; otherwise, <see cref="DefaultInterval"/>.</remarks>
        public override TimeSpan Interval
        {
            get => _interval ?? Config.GetValue<TimeSpan?>(IntervalName) ?? DefaultInterval;
            set => _interval = value;
        }

        /// <summary>
        /// Gets or sets the maximum dequeue size to limit the number of events that are dequeued within a single operation.
        /// </summary>
        /// <remarks>Where specified overrides the <see cref="IEntityOrchestrator.MaxQuerySize"/>.
        /// <para>Will default to configuration <see cref="MaxDequeueSizeName"/>, where specified; otherwise, 10.</para></remarks>
        public int MaxDequeueSize
        {
            get => _maxQuerySize ?? Config.GetValue<int?>(MaxDequeueSizeName) ?? 10;
            set => _maxQuerySize = value;
        }

        /// <summary>
        /// Executes the entity orchestration (<see cref="IEntityOrchestrator.ExecuteAsync(CancellationToken?)"/>) for the next batch and/or last incomplete batch.
        /// </summary>
        /// <param name="scopedServiceProvider"><inheritdoc/></param>
        /// <param name="cancellationToken"><inheritdoc/></param>
        protected override async Task ExecuteAsync(IServiceProvider scopedServiceProvider, CancellationToken cancellationToken)
        {
            // Ensure we have synchronized control; if not exit immediately.
            if (!_synchronizer.Enter<OutboxDequeueHostedService>())
                return;

            // Instantiate the configured IOutboxDequeuePublisher and execute.
            try
            {
                var dp = (scopedServiceProvider ?? throw new ArgumentNullException(nameof(scopedServiceProvider))).GetRequiredService<IOutboxDequeuePublisher>();
                await dp.DequeueAndPublishAsync(MaxDequeueSize, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _synchronizer.Exit<OutboxDequeueHostedService>();
            }
        }
    }
}