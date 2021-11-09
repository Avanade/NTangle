// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System.Threading.Tasks;

namespace NTangle.Events
{
    /// <summary>
    /// Defines the standardised <b>Event</b> publishing and sending. 
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Publishes (queues) one of more <see cref="EventData"/> objects.
        /// </summary>
        /// <param name="events">One or more <see cref="EventData"/> objects.</param>
        /// <returns>The <see cref="IEventPublisher"/> for fluent-style method-chaining.</returns>
        IEventPublisher Publish(params EventData[] events);

        /// <summary>
        /// Sends all previously published events to the corresponding messaging system.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        /// <remarks>This should clear the internal queue of any previously published events after the send.</remarks>
        Task SendAsync();
    }
}