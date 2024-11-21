// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx;
using System.Collections.Generic;
using System.Linq;

namespace NTangle.Data
{
    /// <summary>
    /// Provides the database information.
    /// </summary>
    /// <param name="schema">The schema name.</param>
    /// <param name="table">The table name.</param>
    /// <param name="primaryKeyColumnNames">The primary key column names.</param>
    public class DatabaseInfo(string schema, string table, IEnumerable<string> primaryKeyColumnNames)
    {
        /// <summary>
        /// Gets the schema name.
        /// </summary>
        public string Schema { get; } = schema.ThrowIfNullOrEmpty(nameof(schema));

        /// <summary>
        /// Gets the table name.
        /// </summary>
        public string Table { get; } = table.ThrowIfNullOrEmpty(nameof(table));

        /// <summary>
        /// Gets the primary key column names.
        /// </summary>
        public string[] PrimaryKeyColumnNames { get; } = primaryKeyColumnNames.ThrowIfNull(nameof(primaryKeyColumnNames)).ToArray();
    }
}