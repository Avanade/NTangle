// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Entities;
using CoreEx.Events;

namespace NTangle.Events
{
    /// <summary>
    /// Defines the <see cref="EventDataBase.Subject"/> format.
    /// </summary>
    public enum EventSubjectFormat
    {
        /// <summary>
        /// The subject name appended with the <see cref="IPrimaryKey.PrimaryKey"/>.
        /// </summary>
        NameAndKey,

        /// <summary>
        /// The subject name appended with the <see cref="IGlobalIdentifier.TableKey"/> where exists; otherwise, will fall back to <see cref="IPrimaryKey.PrimaryKey"/>.
        /// </summary>
        NameAndTableKey,

        /// <summary>
        /// The subject name only.
        /// </summary>
        NameOnly
    }
}