// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

namespace NTangle.Events
{
    /// <summary>
    /// Defines the standardised publishing and sending for both outbox and non-outbox events. 
    /// </summary>
    public interface IOutboxEventPublisher : IEventPublisher { }
}