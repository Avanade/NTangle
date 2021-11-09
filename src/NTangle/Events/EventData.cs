// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System;

namespace NTangle.Events
{
    /// <summary>
    /// Represents the core event data.
    /// </summary>
    public class EventData
    {
        /// <summary>
        /// Gets or sets the event subject.
        /// </summary>
        /// <remarks>This is the core subject. Often this will be the name (noun) of the entity being published.</remarks>
        public string? Subject { get; set; }

        /// <summary>
        /// Gets or sets the event action.
        /// </summary>
        /// <remarks>This is the action or command (verb) related to the <see cref="Subject"/>.</remarks>
        public string? Action { get; set; }

        /// <summary>
        /// Gets or sets the event type.
        /// </summary>
        /// <remarks>This describes the type of occurrence which has happened. Often this attribute is used for routing, observability, policy enforcement, etc.</remarks>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the event source.
        /// </summary>
        /// <remarks>This describes the event producer. Often this will include information such as the type of the event source, the organization publishing the event, the process that produced the event, and some unique identifiers.</remarks>
        public Uri? Source { get; set; }

        /// <summary>
        /// Gets or sets the unique event identifier.
        /// </summary>
        /// <remarks>Defaults to the <see cref="string"/> representation of a <see cref="Guid.NewGuid"/>.</remarks>
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the event timestamp.
        /// </summary>
        /// <remarks>Defaults to <see cref="DateTimeOffset.UtcNow"/>.</remarks>
        public DateTimeOffset? Timestamp { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets or sets the event data.
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Gets or sets the primary key.
        /// </summary>
        public CompositeKey PrimaryKey { get; set; }

        /// <summary>
        /// Gets or sets the event correlation identifier.
        /// </summary>
        public string? CorrelationId { get; set; }
    }
}