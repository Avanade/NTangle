// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using DbEx.DbSchema;

namespace NTangle.Config
{
    /// <summary>
    /// Provides the required database table reference properties.
    /// </summary>
    public interface ITableReference
    {
        /// <summary>
        /// Gets the table schema.
        /// </summary>
        string? Schema { get; }

        /// <summary>
        /// Gets the table name.
        /// </summary>
        string? Table { get; }

        /// <summary>
        /// Gets the table alias.
        /// </summary>
        string? Alias { get; }

        /// <summary>
        /// Gets the corresponding <see cref="DbTable"/>.
        /// </summary>
        DbTableSchema? DbTable { get; }
    }
}