// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Entities;
using CoreEx.Events;

namespace NTangle.Events
{
    /// <summary>
    /// Provides the <see cref="EventSubjectFormat"/> formatting capability. 
    /// </summary>
    public static class EventSubjectFormatter
    {
        /// <summary>
        /// Formats the <paramref name="subject"/> based on the specified <paramref name="format"/>.
        /// </summary>
        /// <param name="formatter">The <see cref="EventActionFormatter"/>.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="value">The entity value.</param>
        /// <param name="format">The <see cref="EventSubjectFormat"/>.</param>
        /// <returns>The formatted action.</returns>
        public static string Format<T>(EventDataFormatter formatter, string subject, T value, EventSubjectFormat? format = EventSubjectFormat.NameAndKey) where T : IEntityKey => format switch
        {
            EventSubjectFormat.NameAndKey => $"{subject}{formatter.SubjectSeparatorCharacter}{value.EntityKey.ToString(formatter.KeySeparatorCharacter)}",
            EventSubjectFormat.NameAndTableKey => $"{subject}{formatter.SubjectSeparatorCharacter}{(value is IGlobalIdentifier gi ? gi.TableKey.ToString(formatter.KeySeparatorCharacter) : value.EntityKey.ToString(formatter.KeySeparatorCharacter))}",
            _ => subject
        };
    }
}