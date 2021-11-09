// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NTangle.Events
{
    /// <summary>
    /// Provides the base <b>Event</b> publishing and sending capabilities back by a <see cref="ConcurrentQueue{T}"/>.
    /// </summary>
    public abstract class EventPublisherBase : IEventPublisher
    {
        private readonly Lazy<ConcurrentQueue<EventData>> _queue = new();

        /// <summary>
        /// Gets the published/queued events (dequeues).
        /// </summary>
        /// <returns>An <see cref="EventData"/> array.</returns>
        /// <remarks>As the events are dequeued the results of this method are not idempotent.</remarks>
        public virtual EventData[] GetEvents()
        {
            var list = new List<EventData>();

            while (_queue.Value.TryDequeue(out var ed))
            {
                list.Add(ed);
            }

            return list.ToArray();
        }

        /// <inheritdoc/>
        public virtual IEventPublisher Publish(params EventData[] events)
        {
            foreach (var ed in events)
            {
                _queue.Value.Enqueue(ed);
            }

            return this;
        }

        /// <inheritdoc/>
        /// <remarks>Invokes the <see cref="SendEventsAsync(EventData[])"/> to perform the actual send (all events will be dequeued prior).</remarks>
        public async Task SendAsync()
        {
            var list = new List<EventData>();

            while (_queue.Value.TryDequeue(out var ed))
            {
                list.Add(ed);
            }

            if (list.Count == 0)
                return;

            await SendEventsAsync(list.ToArray()).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends one of more previously published (queued) events (see <see cref="EventData"/>).
        /// </summary>
        /// <param name="events">One or more <see cref="EventData"/> objects.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        /// <remarks>This will not be invoked where there are no events to be sent.</remarks>
        protected abstract Task SendEventsAsync(EventData[] events);
    }
}