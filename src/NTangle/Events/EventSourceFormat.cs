// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

namespace NTangle.Events
{
    /// <summary>
    /// Defines the <see cref="EventData.Source"/> format.
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