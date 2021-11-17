// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using Microsoft.Data.SqlClient;
using System;
using System.Data.Common;

namespace NTangle.Data
{
    /// <summary>
    /// Provides the <b>Microsoft SQL Server</b> specific database access functionality.
    /// </summary>
    public sealed class SqlServerDatabase : Database<SqlConnection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDatabase"/> class.
        /// </summary>
        /// <param name="create">The function to create the <see cref="SqlConnection"/>.</param>
        public SqlServerDatabase(Func<SqlConnection> create) : base(create) { }

        /// <summary>
        /// Gets or sets the <see cref="SqlException.Number"/> that should be converted to a <see cref="DatabaseErrorException"/> via <see cref="OnDbException(DbException)"/>.
        /// </summary>
        public int ErrorNumber { get; set; } = 56002;

        /// <summary>
        /// Invoked where an <see cref="DbException"/> has been thrown.
        /// </summary>
        /// <param name="dbex">The <see cref="DbException"/>.</param>
        /// <remarks>Where the <see cref="SqlException.Number"/> equals the <see cref="ErrorNumber"/> then a <see cref="DatabaseErrorException"/> will be thrown.</remarks>
        public override void OnDbException(DbException dbex)
        {
            if (dbex is SqlException sex && sex.Number == 56002) // The SQL Server magic number is 56002.
                throw new DatabaseErrorException(dbex.Message, dbex);
        }
    }
}