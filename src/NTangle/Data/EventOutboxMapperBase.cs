// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using NTangle.Events;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace NTangle.Data
{
    /// <summary>
    /// Represents the <see cref="EventOutbox"/> database mapper. 
    /// </summary>
    public abstract class EventOutboxMapperBase : IDatabaseMapper<EventOutbox>, IDatabaseTvp<EventOutbox>
    {
        private const string eventIdName = "EventId";

        /// <summary>
        /// Initializes a new instance of the <see cref="EventOutboxMapperBase"/> class.
        /// </summary>
        /// <param name="dbTypeName">The database type name for the <see cref="TableValuedParameter"/>.</param>
        public EventOutboxMapperBase(string dbTypeName) => DbTypeName = dbTypeName ?? throw new ArgumentNullException(nameof(dbTypeName));

        /// <summary>
        /// Gets the database type name for the <see cref="TableValuedParameter"/>.
        /// </summary>
        public string DbTypeName { get; }

        /// <summary>
        /// Gets the event outbox <i>enqueue</i> stored procedure name.
        /// </summary>
        public abstract string EnqueueStoredProcedure { get; }

        /// <summary>
        /// Gets the event outbox <i>dequeue</i> stored procedure name.
        /// </summary>
        public abstract string DequeueStoredProcedure { get; }

        /// <summary>
        /// Enqueues the <see cref="EventOutbox"/> list.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="list">The <see cref="EventOutbox"/> list.</param>
        public async Task EnqueueAsync(IDatabase db, IEnumerable<EventOutbox> list)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            if (list == null || !list.Any())
                return;

            await db.StoredProcedure(EnqueueStoredProcedure, p => p.AddTableValuedParameter("@EventList", CreateTableValuedParameter(list))).NonQueryAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Dequeues the <see cref="EventOutbox"/> list.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="maxDequeueSize">The maximum number of events to dequeue.</param>
        /// <returns>The dequeued <see cref="EventOutbox"/> list.</returns>
        /// <remarks>This should be invoked within the context of a <see cref="IDbTransaction"/> and only committed where the underlying processing of the resulting events has occured successfully to guarantee no message loss.</remarks>
        public async Task<IEnumerable<EventOutbox>> DequeueAsync(IDatabase db, int maxDequeueSize = 10)
            => await (db ?? throw new ArgumentNullException(nameof(db))).StoredProcedure(DequeueStoredProcedure, p => p.AddParameter("@MaxDequeueSize", maxDequeueSize <= 0 ? 10 : maxDequeueSize)).SelectAsync(this).ConfigureAwait(false);

        /// <inheritdoc/>
        public EventOutbox MapFromDb(DatabaseRecord record) => new()
        {
            Id = record.GetValue<string?>(eventIdName),
            Type = record.GetValue<string?>(nameof(EventOutbox.Type)),
            Source = record.GetValue<string?>(nameof(EventOutbox.Source)),
            Timestamp = record.GetValue<DateTimeOffset>(nameof(EventOutbox.Timestamp)),
            CorrelationId = record.GetValue<string?>(nameof(EventOutbox.CorrelationId)),
            EventData = new BinaryData(record.GetValue<byte[]>(nameof(EventOutbox.EventData)))
        };

        /// <inheritdoc/>
        public TableValuedParameter CreateTableValuedParameter(IEnumerable<EventOutbox> list)
        {
            var dt = new DataTable();
            dt.Columns.Add(eventIdName, typeof(string));
            dt.Columns.Add(nameof(EventOutbox.Type), typeof(string));
            dt.Columns.Add(nameof(EventOutbox.Source), typeof(string));
            dt.Columns.Add(nameof(EventOutbox.Timestamp), typeof(DateTimeOffset));
            dt.Columns.Add(nameof(EventOutbox.CorrelationId), typeof(string));
            dt.Columns.Add(nameof(EventOutbox.EventData), typeof(byte[]));

            var tvp = new TableValuedParameter(DbTypeName, dt);
            foreach (var item in list)
            {
                tvp.AddRow(item.Id, item.Type, item.Source, item.Timestamp, item.CorrelationId, item.EventData?.ToArray());
            }

            return tvp;
        }
    }
}