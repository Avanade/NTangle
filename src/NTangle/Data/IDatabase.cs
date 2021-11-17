// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace NTangle.Data
{
    /// <summary>
    /// Defines the database access.
    /// </summary>
    public interface IDatabase : IDisposable
    {
        /// <summary>
        /// Gets the <see cref="DbConnection"/>.
        /// </summary>
        /// <remarks>The connection is created and opened on first use, and closed on <see cref="IDisposable.Dispose()"/>.</remarks>
        Task<DbConnection> GetConnectionAsync();

        /// <summary>
        /// Creates a stored procedure <see cref="DatabaseCommand"/>.
        /// </summary>
        /// <param name="storedProcedure">The stored procedure name.</param>
        /// <param name="parameters">An optional delegate to update the <see cref="DatabaseParameterCollection"/> for the command.</param>
        /// <returns>The <see cref="DatabaseCommand"/>.</returns>
        DatabaseCommand StoredProcedure(string storedProcedure, Action<DatabaseParameterCollection>? parameters = null);

        /// <summary>
        /// Creates a SQL statement <see cref="DatabaseCommand"/>.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="parameters">An optional delegate to update the <see cref="DatabaseParameterCollection"/> for the command.</param>
        /// <returns>The <see cref="DatabaseCommand"/>.</returns>
        DatabaseCommand SqlStatement(string sqlStatement, Action<DatabaseParameterCollection>? parameters = null);

        /// <summary>
        /// Invoked where a <see cref="DbException"/> has been thrown.
        /// </summary>
        /// <param name="dbex">The <see cref="DbException"/>.</param>
        /// <remarks>The <see cref="DbException"/> should be verified to determine whether the exception is related to a <i>known</i> error; if so, then a <see cref="DatabaseErrorException"/> should be thrown; otherwise, the unhandled
        /// exception will continue to bubble up.</remarks>
        void OnDbException(DbException dbex);
    }
}