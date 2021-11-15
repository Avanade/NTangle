// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System;

namespace NTangle.Events
{
    /// <summary>
    /// Represents the <see cref="EventData"/> formatted for an event outbox.
    /// </summary>
    public class EventOutbox
    {
        /// <summary>
        /// Gets or sets the unique event identifier (<see cref="EventData.Id"/>).
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the event type (<see cref="EventData.Type"/>).
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the event source (<see cref="EventData.Source"/>).
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Gets or sets the event timestamp (<see cref="EventData.Timestamp"/>).
        /// </summary>
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the correlation identifier (<see cref="EventData.CorrelationId"/>).
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the serialized <see cref="EventData"/> (see <see cref="IEventSerializer"/>).
        /// </summary>
        public BinaryData? EventData { get; set; }
    }
}