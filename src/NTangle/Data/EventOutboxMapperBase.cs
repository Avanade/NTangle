// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using DbEx;
using DbEx.SqlServer;
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
        /// Gets or sets the default partition key.
        /// </summary>
        /// <remarks>Defaults to '<c>default</c>'. This will ensure that there is always a value recorded in the database.</remarks>
        public string DefaultPartitionKey { get; set; } = "default";

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
        /// <param name="maxDequeueSize">The maximum number of events to dequeue; defaults to 50.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>The dequeued <see cref="EventOutbox"/> list.</returns>
        /// <remarks>This should be invoked within the context of a <see cref="IDbTransaction"/> and only committed where the underlying processing of the resulting events has occured successfully to guarantee no message loss.</remarks>
        public async Task<IEnumerable<EventOutbox>> DequeueAsync(IDatabase db, int maxDequeueSize = 50, string? partitionKey = null) => await (db ?? throw new ArgumentNullException(nameof(db)))
            .StoredProcedure(DequeueStoredProcedure, p => p.Param("@MaxDequeueSize", maxDequeueSize <= 0 ? 50 : maxDequeueSize).Param("@PartitionKey", partitionKey))
            .SelectAsync(this).ConfigureAwait(false);

        /// <inheritdoc/>
        public EventOutbox MapFromDb(DatabaseRecord record)
        {
            var source = record.GetValue<string?>(nameof(EventData.Source));

            return new()
            {
                Id = record.GetValue<string?>(eventIdName),
                Subject = record.GetValue<string?>(nameof(EventData.Subject)),
                Action = record.GetValue<string?>(nameof(EventData.Action)),
                Type = record.GetValue<string?>(nameof(EventData.Type)),
                Source = string.IsNullOrEmpty(source) ? null : new Uri(source, UriKind.RelativeOrAbsolute),
                Timestamp = record.GetValue<DateTimeOffset>(nameof(EventData.Timestamp)),
                CorrelationId = record.GetValue<string?>(nameof(EventData.CorrelationId)),
                TenantId = record.GetValue<string?>(nameof(EventData.TenantId)),
                PartitionKey = record.GetValue<string?>(nameof(EventData.PartitionKey)),
                Data = new BinaryData(record.GetValue<byte[]>(nameof(EventData.Data)))
            };
        }

        /// <inheritdoc/>
        public TableValuedParameter CreateTableValuedParameter(IEnumerable<EventOutbox> list)
        {
            var dt = new DataTable();
            dt.Columns.Add(eventIdName, typeof(string));
            dt.Columns.Add(nameof(EventData.Subject), typeof(string));
            dt.Columns.Add(nameof(EventData.Action), typeof(string));
            dt.Columns.Add(nameof(EventData.Type), typeof(string));
            dt.Columns.Add(nameof(EventData.Source), typeof(string));
            dt.Columns.Add(nameof(EventData.Timestamp), typeof(DateTimeOffset));
            dt.Columns.Add(nameof(EventData.CorrelationId), typeof(string));
            dt.Columns.Add(nameof(EventData.TenantId), typeof(string));
            dt.Columns.Add(nameof(EventData.PartitionKey), typeof(string));
            dt.Columns.Add(nameof(EventData.Data), typeof(byte[]));

            var tvp = new TableValuedParameter(DbTypeName, dt);
            foreach (var item in list)
            {
                tvp.AddRow(item.Id, item.Subject, item.Action, item.Type, item.Source, item.Timestamp, item.CorrelationId, item.TenantId, item.PartitionKey ?? DefaultPartitionKey, item.Data?.ToArray());
            }

            return tvp;
        }
    }
}