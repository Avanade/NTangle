// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using NTangle.Events;
using System.Threading;
using System.Threading.Tasks;

namespace NTangle.Data
{
    /// <summary>
    /// Defines the <see cref="EventOutbox"/> dequeue and corresponding <see cref="IOutboxEventPublisher">publishing</see> capabilities.
    /// </summary>
    public interface IOutboxDequeuePublisher
    {
        /// <summary>
        /// Performs the dequeue of the events from the database outbox and then publishes.
        /// </summary>
        /// <param name="maxDequeueSize">The maximum dequeue size.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        Task DequeueAndPublishAsync(int maxDequeueSize, CancellationToken cancellationToken);
    }
}