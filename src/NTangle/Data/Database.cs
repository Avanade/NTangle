// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace NTangle.Data
{
    /// <summary>
    /// Provides the base database access functionality.
    /// </summary>
    /// <typeparam name="TConnection">The <see cref="DbConnection"/> <see cref="Type"/>.</typeparam>
    public abstract class Database<TConnection> : IDatabase, IDisposable where TConnection : DbConnection
    {
        private readonly Func<TConnection> _dbConnCreate;
        private TConnection? _dbConn;

        /// <summary>
        /// Initializes a new instance of the <see cref="Database{TConn}"/> class.
        /// </summary>
        /// <param name="create">The function to create the <typeparamref name="TConnection"/> <see cref="DbConnection"/>.</param>
        protected Database(Func<TConnection> create) => _dbConnCreate = create ?? throw new ArgumentNullException(nameof(create));

        /// <summary>
        /// Gets the <typeparamref name="TConnection"/> <see cref="DbConnection"/>.
        /// </summary>
        /// <remarks>The connection is created and opened on first use, and closed on <see cref="IDisposable.Dispose()"/>.</remarks>
        public async Task<TConnection> GetConnectionAsync()
        {
            if (_dbConn == null)
            {
                _dbConn = _dbConnCreate() ?? throw new InvalidOperationException($"The create function must create a valid {nameof(TConnection)} instance.");
                await _dbConn.OpenAsync().ConfigureAwait(false);
            }

            return _dbConn;
        }

        /// <inheritdoc/>
        async Task<DbConnection> IDatabase.GetConnectionAsync() => await GetConnectionAsync().ConfigureAwait(false);

        /// <inheritdoc/>
        public DatabaseCommand StoredProcedure(string storedProcedure, Action<DatabaseParameterCollection>? parameters = null)
            => new(this, CommandType.StoredProcedure, storedProcedure ?? throw new ArgumentNullException(nameof(storedProcedure)), parameters);

        /// <inheritdoc/>
        public DatabaseCommand SqlStatement(string sqlStatement, Action<DatabaseParameterCollection>? parameters = null)
            => new(this, CommandType.Text, sqlStatement ?? throw new ArgumentNullException(nameof(sqlStatement)), parameters);

        /// <inheritdoc/>
        public virtual void OnDbException(DbException dbex) { }

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