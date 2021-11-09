// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace NTangle.Data
{
    /// <summary>
    /// Provides the database access.
    /// </summary>
    public class Database : IDatabase, IDisposable
    {
        private readonly Func<DbConnection> _dbConnCreate;
        private DbConnection? _dbConn;

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class.
        /// </summary>
        /// <param name="create">The function to create the <see cref="DbConnection"/>.</param>
        public Database(Func<DbConnection> create) => _dbConnCreate = create ?? throw new ArgumentNullException(nameof(create));

        /// <inheritdoc/>
        public async Task<DbConnection> GetConnectionAsync()
        {
            if (_dbConn == null)
            {
                _dbConn = _dbConnCreate() ?? throw new InvalidOperationException($"The create function must create a valid {nameof(DbConnection)} instance.");
                await _dbConn.OpenAsync().ConfigureAwait(false);
            }

            return _dbConn;
        }

        /// <inheritdoc/>
        public DatabaseCommand StoredProcedure(string storedProcedure, Action<DatabaseParameterCollection>? parameters = null)
            => new DatabaseCommand(this, CommandType.StoredProcedure, storedProcedure ?? throw new ArgumentNullException(nameof(storedProcedure)), parameters);

        /// <inheritdoc/>
        public DatabaseCommand SqlStatement(string sqlStatement, Action<DatabaseParameterCollection>? parameters = null)
            => new DatabaseCommand(this, CommandType.Text, sqlStatement ?? throw new ArgumentNullException(nameof(sqlStatement)), parameters);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the resources.
        /// </summary>
        /// <param name="disposing">Indicates whether to dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _dbConn != null)
            {
                _dbConn.Dispose();
                _dbConn = null;
            }
        }
    }
}