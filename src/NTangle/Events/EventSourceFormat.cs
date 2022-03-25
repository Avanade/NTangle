// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Entities;
using CoreEx.Events;

namespace NTangle.Events
{
    /// <summary>
    /// Defines the <see cref="EventDataBase.Source"/> format.
    /// </summary>
    public enum EventSourceFormat
    {
        /// <summary>
        /// The source will be set to <c>Null</c>.
        /// </summary>
        None,

        /// <summary>
        /// The source name appended with the <see cref="IPrimaryKey.PrimaryKey"/>.
        /// </summary>
        NameAndKey,

        /// <summary>
        /// The source name appended with the <see cref="IGlobalIdentifier.TableKey"/> where exists; otherwise, will fall back to <see cref="IPrimaryKey.PrimaryKey"/>.
        /// </summary>
        NameAndTableKey,

        /// <summary>
        /// The source name only.
        /// </summary>
        NameOnly
    }
}