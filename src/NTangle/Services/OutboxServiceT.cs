// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NTangle.Cdc;
using NTangle.Data;
using NTangle.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace NTangle.Services
{
    /// <summary>
    /// Provides the <see cref="EventOutbox"/> dequeue and publish <see cref="TimerHostedServiceBase"/> capabilities.
    /// </summary>
    public class OutboxService<TMapper> : OutboxService where TMapper : EventOutboxMapperBase, new()
    {
        private readonly IServiceSynchronizer _synchronizer;
        private readonly TMapper _mapper = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="HostedService"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <param name="synchronizer"> The <see cref="IServiceSynchronizer"/>.</param>
        public OutboxService(IServiceProvider serviceProvider, ILogger logger, IConfiguration config, IServiceSynchronizer synchronizer) : base(serviceProvider, logger, config)
            => _synchronizer = synchronizer ?? throw new ArgumentNullException(nameof(synchronizer));

        /// <summary>
        /// Executes the entity orchestration (<see cref="IEntityOrchestrator.ExecuteAsync(CancellationToken?)"/>) for the next batch and/or last incomplete batch.
        /// </summary>
        /// <param name="scopedServiceProvider"><inheritdoc/></param>
        /// <param name="cancellationToken"><inheritdoc/></param>
        protected override async Task ExecuteAsync(IServiceProvider scopedServiceProvider, CancellationToken cancellationToken)
        {
            // Ensure we have synchronized control; if not exit immediately.
            if (!_synchronizer.Enter<OutboxService>())
                return;

            try
            {
                // Get the required scoped services.
                var db = GetService<IDatabase>(scopedServiceProvider);
                var publisher = GetService<IOutboxEventPublisher>(scopedServiceProvider);
                Stopwatch sw = new();

                // Keep executing until unsuccessful or reached end of event outbox stream.
                while (true)
                {
                    // Where a cancel has been requested then this is a convenient time to do it.
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    // Manage a transaction to ensure that the dequeue only commits after successful publish.
                    using var txn = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                    // Dequeue the events; where there are none to publish, then simply exit and try again later.
                    Logger.LogTrace("Dequeue events. [MaxDequeueSize={MaxDequeueSize}]", MaxDequeueSize);

                    sw = Stopwatch.StartNew();
                    var events = await _mapper.DequeueAsync(db, MaxDequeueSize).ConfigureAwait(false);
                    sw.Stop();

                    if (events == null || !events.Any())
                    {
                        txn.Complete();
                        return;
                    }

                    Logger.LogInformation("{EventCount} event(s) were dequeued. [Elapsed={Elapsed}ms]", events.Count(), sw.ElapsedMilliseconds);

                    // Deserilaize the events.
                    var list = new List<EventData>();
                    foreach (var @event in events)
                    {
                        if (@event.EventData == null)
                            continue;

                        var ed = JsonConvert.DeserializeObject<EventData>(@event.EventData.ToString());
                        if (ed != null)
                            list.Add(ed);
                    }

                    // Publish the events.
                    if (list.Count > 0)
                    {
                        sw = Stopwatch.StartNew();
                        await publisher.SendAsync(list.ToArray()).ConfigureAwait(false);
                        sw.Stop();
                        Logger.LogInformation("{EventCount} event(s) were published successfully. [Publisher={Publisher}, Elapsed={Elapsed}ms]", list.Count, publisher.GetType().Name, sw.ElapsedMilliseconds);
                    }

                    // Commit the transaction.
                    txn.Complete();
                }
            }
            finally
            {
                _synchronizer.Exit<OutboxService>();
            }
        }

        /// <summary>
        /// Get the requested instance from the scoped service provider.
        /// </summary>
        private static T GetService<T>(IServiceProvider scopedServiceProvider)
            => (T)scopedServiceProvider.GetService(typeof(T)) ?? throw new InvalidOperationException($"Attempted to get service '{typeof(T).FullName}' but null was returned; this would indicate that the service has not been configured correctly.");
    }
}