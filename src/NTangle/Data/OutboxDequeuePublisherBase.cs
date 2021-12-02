// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using DbEx;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NTangle.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace NTangle.Data
{
    /// <summary>
    /// Enables the <see cref="EventOutbox"/> dequeue and corresponding <see cref="IOutboxEventPublisher">publishing</see> capabilities.
    /// </summary>
    /// <typeparam name="TMapper">The <see cref="EventOutboxMapperBase"/> <see cref="Type"/>.</typeparam>
    public abstract class OutboxDequeuePublisherBase<TMapper> : IOutboxDequeuePublisher where TMapper : EventOutboxMapperBase, new()
    {
        private readonly IDatabase _db;
        private readonly IEventPublisher _publisher;
        private readonly ILogger _logger;
        private readonly TMapper _mapper = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="OutboxDequeuePublisherBase{TMapper}"/> class.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="publisher">The <see cref="IOutboxEventPublisher"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public OutboxDequeuePublisherBase(IDatabase db, IOutboxEventPublisher publisher, ILogger logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Performs the dequeue of the events from the database outbox and then publishes.
        /// </summary>
        /// <param name="maxDequeueSize">The maximum dequeue size.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        public async Task DequeueAndPublishAsync(int maxDequeueSize, CancellationToken cancellationToken)
        {
            Stopwatch sw;
            maxDequeueSize = maxDequeueSize > 0 ? maxDequeueSize : 1;

            // Keep executing until unsuccessful or reached end of event outbox stream.
            while (true)
            {
                // Where a cancel has been requested then this is a convenient time to do it.
                if (cancellationToken.IsCancellationRequested)
                    return;

                // Manage a transaction to ensure that the dequeue only commits after successful publish.
                using var txn = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                // Dequeue the events; where there are none to publish, then simply exit and try again later.
                _logger.LogTrace("Dequeue events. [MaxDequeueSize={MaxDequeueSize}]", maxDequeueSize);

                sw = Stopwatch.StartNew();
                var events = await _mapper.DequeueAsync(_db, maxDequeueSize).ConfigureAwait(false);
                sw.Stop();

                if (events == null || !events.Any())
                {
                    txn.Complete();
                    return;
                }

                _logger.LogInformation("{EventCount} event(s) were dequeued. [Elapsed={Elapsed}ms]", events.Count(), sw.ElapsedMilliseconds);

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
                    await _publisher.SendAsync(list.ToArray()).ConfigureAwait(false);
                    sw.Stop();
                    _logger.LogInformation("{EventCount} event(s) were published successfully. [Publisher={Publisher}, Elapsed={Elapsed}ms]", list.Count, _publisher.GetType().Name, sw.ElapsedMilliseconds);
                }

                // Commit the transaction.
                txn.Complete();
            }
        }
    }
}