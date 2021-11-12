// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System.Threading.Tasks;

namespace NTangle.Events
{
    /// <summary>
    /// Represents a <c>null</c> event publisher; whereby the events are simply swallowed/discarded on send.
    /// </summary>
    public class NullEventPublisher : IEventPublisher
    {
        /// <inheritdoc/>
        public Task SendAsync(params EventData[] events) => Task.CompletedTask;
    }
}