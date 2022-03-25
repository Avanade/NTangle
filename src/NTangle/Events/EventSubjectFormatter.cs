// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Entities;

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
        /// <param name="subject">The subject.</param>
        /// <param name="value">The entity value.</param>
        /// <param name="format">The <see cref="EventSubjectFormat"/>.</param>
        /// <returns>The formatted action.</returns>
        public static string Format<T>(string subject, T value, EventSubjectFormat? format = EventSubjectFormat.NameAndKey) where T : IPrimaryKey => format switch
        {
            EventSubjectFormat.NameAndKey => $"{subject}.{value.PrimaryKey}",
            EventSubjectFormat.NameAndTableKey => $"{subject}.{(value is IGlobalIdentifier gi ? gi.TableKey : value.PrimaryKey)}",
            _ => subject
        };
    }
}