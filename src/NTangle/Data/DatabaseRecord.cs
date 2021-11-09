// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System;
using System.Data.Common;

namespace NTangle.Data
{
    /// <summary>
    /// Encapsulates the <see cref="DbDataReader"/> to provide requisite column value capabilities.
    /// </summary>
    public class DatabaseRecord
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseRecord"/> class.
        /// </summary>
        /// <param name="dataReader">The underlying <see cref="DbDataReader"/>.</param>
        public DatabaseRecord(DbDataReader dataReader) => DataReader = dataReader ?? throw new ArgumentNullException(nameof(dataReader));

        /// <summary>
        /// Gets the underlying <see cref="DbDataReader"/>.
        /// </summary>
        public DbDataReader DataReader { get; private set; }

        /// <summary>
        /// Gets the named column value.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="columnName">The column name.</param>
        /// <returns>The value.</returns>
        public T GetValue<T>(string columnName)
        {
            var i = DataReader.GetOrdinal(columnName ?? throw new ArgumentNullException(nameof(columnName)));
            if (DataReader.IsDBNull(i))
                return default!;

            return DataReader.GetFieldValue<T>(i);

            //var nt = Nullable.GetUnderlyingType(typeof(T));
            //if (nt == null)
            //    return DataReader.GetFieldValue<T>(i);
            //else
            //    return (T)Convert.ChangeType(DataReader.GetValue(i), nt, System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Indicates whether the named column is <see cref="DBNull"/>.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <returns><c>true</c> indicates that the column value has a <see cref="DBNull"/> value; otherwise, <c>false</c>.</returns>
        public bool IsDBNull(string columnName)
        {
            var i = DataReader.GetOrdinal(columnName ?? throw new ArgumentNullException(nameof(columnName)));
            return DataReader.IsDBNull(i);
        }

        /// <summary>
        /// Gets the named <c>RowVersion</c> column as a <see cref="string"/>.
        /// </summary>
        /// <param name="columnName">The name of the column.</param>
        /// <returns>The resultant value.</returns>
        /// <remarks>The <b>RowVersion</b> <see cref="byte"/> array will be converted to an <see cref="Convert.ToBase64String(byte[])">encoded</see> <see cref="string"/> value.</remarks>
        public string GetRowVersion(string columnName)
        {
            var i = DataReader.GetOrdinal(columnName ?? throw new ArgumentNullException(nameof(columnName)));
            return Convert.ToBase64String(DataReader.GetFieldValue<byte[]>(i));
        }
    }
}