// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System;
using System.Threading.Tasks;

namespace NTangle.Events
{
    /// <summary>
    /// Defines an <see cref="EventData"/> serializer.
    /// </summary>
    public interface IEventSerializer
    {
        /// <summary>
        /// Serializes an <see cref="EventData"/> to a <see cref="byte"/> array.
        /// </summary>
        /// <param name="event">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="BinaryData"/>.</returns>
        Task<BinaryData> SerializeAsync(EventData @event);
    }
}