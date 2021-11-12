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
        /// Sends one of more <see cref="EventData"/> objects.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        Task SendAsync(params EventData[] events);
    }
}