// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System;

namespace NTangle.Events
{
    /// <summary>
    /// Represents the <see cref="Events.EventData"/> formatted for an event outbox.
    /// </summary>
    public class EventOutbox : EventData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventOutbox"/> class.
        /// </summary>
        public EventOutbox() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventOutbox"/> class copying from another <paramref name="event"/> excluding the underlying <see cref="Data"/>.
        /// </summary>
        public EventOutbox(EventData @event) : base(@event) { }

        /// <summary>
        /// Gets or sets the serialized <see cref="EventData.Data"/>.
        /// </summary>
        public new BinaryData? Data { get => (BinaryData?)base.Data!; set => base.Data = value; }
    }
}