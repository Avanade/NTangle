// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using Newtonsoft.Json;
using NTangle.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NTangle.Events
{
    /// <summary>
    /// Enables the <see cref="EventOutbox"/> <see cref="IEventPublisher"/>.
    /// </summary>
    /// <typeparam name="TMapper">The <see cref="EventOutboxMapperBase"/> <see cref="Type"/>.</typeparam>
    /// <remarks>This serializes the <see cref="EventData"/> directly to JSON and converts to <see cref="BinaryData"/>; versus, using an <see cref="IEventSerializer"/>. This is so that it be easily deserialized when dequeueing and
    /// and a further <see cref="IEventPublisher"/> used to send which may then in turn leverage an <see cref="IEventSerializer"/>.</remarks>
    public abstract class OutboxEventPublisherBase<TMapper> : IEventPublisher where TMapper : EventOutboxMapperBase, new()
    {
        private readonly IDatabase _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutboxEventPublisherBase{TMapper}"/> class.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        public OutboxEventPublisherBase(IDatabase db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        /// <inheritdoc/>
        public async Task SendAsync(params EventData[] events)
        {
            var mapper = new TMapper();
            var list = new List<EventOutbox>();
            foreach (var @event in events)
            {
                list.Add(new EventOutbox
                {
                    Id = @event.Id,
                    Type = @event.Type,
                    Source = @event.Source?.ToString(),
                    Timestamp = @event.Timestamp,
                    EventData = new BinaryData(JsonConvert.SerializeObject(@event))
                });
            }

            await mapper.EnqueueAsync(_db, list).ConfigureAwait(false);
        }
    }
}