// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

namespace NTangle
{
    /// <summary>
    /// Provides the <see cref="PrimaryKey"/>.
    /// </summary>
    public interface IPrimaryKey
    {
        /// <summary>
        /// Gets the <i>primary key</i> (represented as a <see cref="CompositeKey"/>).
        /// </summary>
        CompositeKey PrimaryKey { get; }
    }
}