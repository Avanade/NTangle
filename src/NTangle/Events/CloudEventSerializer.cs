// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CloudNative.CloudEvents;
using CloudNative.CloudEvents.NewtonsoftJson;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;

namespace NTangle.Events
{
    /// <summary>
    /// Serializes an <see cref="EventData"/> as a JSON-formatted <see href="https://cloudevents.io/">CloudEvent</see>.
    /// </summary>
    public class CloudEventSerializer : IEventSerializer
    {
        /// <inheritdoc/>
        public Task<byte[]> SerializeAsync(EventData @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            var ce = new CloudEvent
            {
                Type = @event.Type?.ToLowerInvariant(),
                Source = @event.Source,
                Id = @event.Id,
                Time = @event.Timestamp,
            };

            SetExtensionAttribute(ce, "subject", @event.Subject?.ToLowerInvariant());
            SetExtensionAttribute(ce, "action", @event.Action?.ToLowerInvariant());
            SetExtensionAttribute(ce, "correlationid", @event.CorrelationId);

            OnSerialize(@event, ce);

            ce.DataContentType = MediaTypeNames.Application.Json;
            ce.Data = @event.Data;

            return Task.FromResult(new JsonEventFormatter().EncodeStructuredModeMessage(ce, out var _).ToArray());
        }

        /// <summary>
        /// Invoked after the standard <see cref="EventData"/> properties have been updated to the <see cref="CloudEvent"/> to enable further customization where required.
        /// </summary>
        /// <param name="event">The source <see cref="EventData"/>.</param>
        /// <param name="cloudEvent">The corresponding <see cref="CloudEvent"/>.</param>
        protected virtual void OnSerialize(EventData @event, CloudEvent cloudEvent) { }

        /// <summary>
        /// Sets the <see cref="CloudEvent"/> extension attribute where not <c>null</c>.
        /// </summary>
        /// <param name="ce">The <see cref="CloudEvent"/>.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The attribute value.</param>
        public static void SetExtensionAttribute<T>(CloudEvent ce, string name, T value)
        {
            if (Comparer<T>.Default.Compare(value, default!) == 0)
                return;

            ce[name] = value;
        }
    }
}