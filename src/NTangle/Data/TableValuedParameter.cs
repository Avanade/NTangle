// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System;
using System.Data;

namespace NTangle.Data
{
    /// <summary>
    /// Represents a SQL-Server table-valued parameter (see <see href="https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/table-valued-parameters"/>).
    /// </summary>
    public class TableValuedParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableValuedParameter"/> class.
        /// </summary>
        /// <param name="typeName">The SQL type name of the table-valued parameter.</param>
        /// <param name="value">The <see cref="DataTable"/> value.</param>
        public TableValuedParameter(string typeName, DataTable value)
        {
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the SQL type name of the table-valued parameter.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Gets or sets the <see cref="DataTable"/> value.
        /// </summary>
        public DataTable Value { get; }

        /// <summary>
        /// Adds a new <see cref="DataRow"/> to the <see cref="Value"/> using the specified <paramref name="columnValues"/>.
        /// </summary>
        /// <param name="columnValues">The column values.</param>
        public void AddRow(params object?[] columnValues)
        {
            var r = Value.NewRow();
            for (int i = 0; i < columnValues.Length; i++)
            {
                r[i] = columnValues[i] ?? DBNull.Value;
            }

            Value.Rows.Add(r);
        }
    }
}