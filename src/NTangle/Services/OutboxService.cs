// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NTangle.Cdc;
using NTangle.Events;
using System;

namespace NTangle.Services
{
    /// <summary>
    /// Provides the base <see cref="EventOutbox"/> dequeue and publish <see cref="TimerHostedServiceBase"/> capabilities.
    /// </summary>
    public abstract class OutboxService : TimerHostedServiceBase
    {
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
        internal OutboxService(IServiceProvider serviceProvider, ILogger logger, IConfiguration config) : base(serviceProvider, logger, config) { }

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
        public virtual int MaxDequeueSize
        {
            get => _maxQuerySize ?? Config.GetValue<int?>(MaxDequeueSizeName) ?? 10;
            set => _maxQuerySize = value;
        }
    }
}