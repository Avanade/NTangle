// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Entities;
using CoreEx.Events;
using System;

namespace NTangle.Events
{
    /// <summary>
    /// Provides the <see cref="EventSourceFormat"/> formatting capability. 
    /// </summary>
    public static class EventSourceFormatter
    {
        /// <summary>
        /// Formats the <paramref name="source"/> based on the specified <paramref name="format"/>.
        /// </summary>
        /// <param name="formatter">The <see cref="EventActionFormatter"/>.</param>
        /// <param name="source">The source <see cref="Uri"/>.</param>
        /// <param name="value">The entity value.</param>
        /// <param name="format">The <see cref="EventSourceFormat"/>.</param>
        /// <returns>The formatted action.</returns>
        public static Uri? Format<T>(EventDataFormatter formatter, Uri? source, T value, EventSourceFormat? format = EventSourceFormat.NameAndTableKey) where T : IEntityKey
        {
            if (source == null)
                return null;

            return format switch
            {
                EventSourceFormat.NameOnly => source,
                EventSourceFormat.NameAndKey => CreateUri(source, value.EntityKey.ToString(formatter.KeySeparatorCharacter)),
                EventSourceFormat.NameAndTableKey => CreateUri(source, value is IGlobalIdentifier gi ? gi.TableKey.ToString(formatter.KeySeparatorCharacter) : value.EntityKey.ToString(formatter.KeySeparatorCharacter)),
                _ => null
            };
        }

        /// <summary>
        /// Create new extended URI.
        /// </summary>
        private static Uri CreateUri(Uri source, string path) => new($"{source.OriginalString}/{path}", source.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
    }
}