// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Entities;
using System;

namespace NTangle
{
    /// <summary>
    /// Provides the <see cref="GlobalId"/> for a class.
    /// </summary>
    public interface IGlobalIdentifier
    {
        /// <summary>
        /// Gets or sets the global identifier.
        /// </summary>
        object GlobalId { get; set; }

        /// <summary>
        /// Gets the corresponding database table <i>primary key</i> (represented as an <see cref="CompositeKey"/>) as an alternative to the <see cref="IPrimaryKey.PrimaryKey"/> which should be set to <see cref="GlobalId"/>.
        /// </summary>
        /// <remarks>The underlying <see cref="CompositeKey.IsInitial">IsInitial</see> will be <c>true</c> when the record no longer exists within the database (i.e. has been deleted).</remarks>
        CompositeKey TableKey { get; }
    }

    /// <summary>
    /// Provides the <see cref="GlobalId"/> for a class.
    /// </summary>
    /// <typeparam name="T">The global identifier <see cref="Type"/>.</typeparam>
    public interface IGlobalIdentifier<T> : IGlobalIdentifier
    {
        /// <summary>
        /// Gets or sets the global identifier.
        /// </summary>
        object IGlobalIdentifier.GlobalId { get => GlobalId!; set => GlobalId = (T)value; }

        /// <summary>
        /// Gets or sets the global identifier.
        /// </summary>
        new T GlobalId { get; set; }
    }
}