// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace NTangle.Data
{
    /// <summary>
    /// Provides the database command capabilities.
    /// </summary>
    public sealed class DatabaseCommand
    {
        private readonly Action<DatabaseParameterCollection>? _parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseCommand"/> class.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="commandType">The <see cref="System.Data.CommandType"/>.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">An optional delegate to update the <see cref="DatabaseParameterCollection"/> for the command.</param>
        public DatabaseCommand(IDatabase db, CommandType commandType, string commandText, Action<DatabaseParameterCollection>? parameters = null)
        {
            Database = db ?? throw new ArgumentNullException(nameof(db));
            CommandType = commandType;
            CommandText = commandText ?? throw new ArgumentNullException(nameof(commandText));
            _parameters = parameters;
        }

        /// <summary>
        /// Gets the underlying <see cref="IDatabase"/>.
        /// </summary>
        public IDatabase Database { get; }

        /// <summary>
        /// Gets the <see cref="System.Data.CommandType"/>.
        /// </summary>
        public CommandType CommandType { get; }

        /// <summary>
        /// Gets the command text.
        /// </summary>
        public string CommandText { get; }

        /// <summary>
        /// Creates the corresponding <see cref="DbCommand"/>.
        /// </summary>
        /// <returns></returns>
        private async Task<DbCommand> CreateDbCommandAsync()
        {
            var cmd = (await Database.GetConnectionAsync().ConfigureAwait(false)).CreateCommand();
            cmd.CommandType = CommandType;
            cmd.CommandText = CommandText;

            // Action the parameters.
            _parameters?.Invoke(new DatabaseParameterCollection(cmd));

            return cmd;
        }

        /// <summary>
        ///  Executes a multi-dataset query command with one or more <see cref="IMultiSetArgs"/>.
        /// </summary>
        /// <param name="multiSetArgs">One or more <see cref="IMultiSetArgs"/>.</param>
        /// <remarks>The number of <see cref="IMultiSetArgs"/> specified must match the number of returned datasets. A null dataset indicates to ignore (skip) a dataset.</remarks>
        public async Task SelectMultiSetAsync(params IMultiSetArgs[] multiSetArgs)
        {
            if (multiSetArgs == null || multiSetArgs.Length == 0)
                throw new ArgumentException($"At least one {nameof(IMultiSetArgs)} must be supplied.", nameof(multiSetArgs));

            try
            {
                // Create and execute the command. 
                using var cmd = await CreateDbCommandAsync().ConfigureAwait(false);
                using var dr = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

                // Iterate through the dataset(s).
                var index = 0;
                var records = 0;
                IMultiSetArgs? multiSetArg = null;
                do
                {
                    if (index >= multiSetArgs.Length)
                        throw new InvalidOperationException($"{nameof(SelectMultiSetAsync)}  has returned more record sets than expected ({multiSetArgs.Length}).");

                    if (multiSetArgs[index] != null)
                    {
                        records = 0;
                        multiSetArg = multiSetArgs[index];
                        while (await dr.ReadAsync().ConfigureAwait(false))
                        {
                            records++;
                            if (multiSetArg.MaxRows.HasValue && records > multiSetArg.MaxRows.Value)
                                throw new InvalidOperationException($"{nameof(SelectMultiSetAsync)} (multiSetArgs[{index}]) has returned more records than expected ({multiSetArg.MaxRows.Value}).");

                            multiSetArg.DatasetRecord(new DatabaseRecord(dr));
                        }

                        if (records < multiSetArg.MinRows)
                            throw new InvalidOperationException($"{nameof(SelectMultiSetAsync)}  (multiSetArgs[{index}]) has returned less records ({records}) than expected ({multiSetArg.MinRows}).");

                        if (records == 0 && multiSetArg.StopOnNull)
                            return;

                        multiSetArg.InvokeResult();
                    }

                    index++;
                } while (dr.NextResult());

                if (index < multiSetArgs.Length && !multiSetArgs[index].StopOnNull)
                    throw new InvalidOperationException($"{nameof(SelectMultiSetAsync)}  has returned less ({index}) record sets than expected ({multiSetArgs.Length}).");
            }
            catch (DbException dbex)
            {
                ThrowIfError(dbex);
                throw;
            }
        }

        /// <summary>
        /// Check the <see cref="DbException"/> and get underlying error and throw if a known NTangle database error.
        /// </summary>
        private static void ThrowIfError(DbException dbex)
        {
            if (dbex is SqlException sex && sex.Number == 56002) // The SQL Server magic number is 56002.
                throw new DatabaseErrorException(dbex.Message, dbex);
        }

        /// <summary>
        /// Executes a non-query command.
        /// </summary>
        /// <param name="parameters">The post-execution delegate to enable parameter access.</param>
        /// <returns>The number of rows affected.</returns>
        public async Task<int> NonQueryAsync(Action<DbParameterCollection>? parameters = null) => await ExecuteWrapper(async () =>
        {
            // Create and execute the command. 
            using var cmd = await CreateDbCommandAsync().ConfigureAwait(false);
            var result = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            parameters?.Invoke(cmd.Parameters);
            return result;
        });

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query.
        /// </summary>
        /// <typeparam name="T">The result <see cref="Type"/>.</typeparam>
        /// <param name="parameters">The post-execution delegate to enable parameter access.</param>
        /// <returns>The value of the first column of the first row in the result set.</returns>
        public async Task<T> ScalarAsync<T>(Action<DbParameterCollection>? parameters = null) => await ExecuteWrapper(async () =>
        {
            // Create and execute the command. 
            using var cmd = await CreateDbCommandAsync().ConfigureAwait(false);
            var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
            T value = result is null ? default! : result is DBNull ? default! : (T)result;
            parameters?.Invoke(cmd.Parameters);
            return value;
        });

        /// <summary>
        /// Wrap the execution and manage the <see cref="DbConnection"/> handling.
        /// </summary>
        private static async Task<T> ExecuteWrapper<T>(Func<Task<T>> func)
        {
            try
            {
                return await func().ConfigureAwait(false);
            }
            catch (DbException dbex)
            {
                ThrowIfError(dbex);
                throw;
            }
        }

        /// <summary>
        /// Selects a single item.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <returns>The single item.</returns>
        public async Task<T> SelectSingleAsync<T>(IDatabaseMapper<T> mapper)
        {
            T item = await SelectSingleFirstAsync(mapper, true).ConfigureAwait(false);
            if (Comparer<T>.Default.Compare(item, default!) == 0)
                throw new InvalidOperationException("SelectSingle request has not returned a row.");

            return item;
        }

        /// <summary>
        /// Selects a single item or default.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <returns>The single item or default.</returns>
        public async Task<T> SelectSingleOrDefaultAsync<T>(IDatabaseMapper<T> mapper) => await SelectSingleFirstAsync(mapper, false).ConfigureAwait(false);

        /// <summary>
        /// Selects first item.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <returns>The single item.</returns>
        public async Task<T> SelectFirstAsync<T>(IDatabaseMapper<T> mapper)
        {
            T item = await SelectSingleFirstAsync(mapper, false).ConfigureAwait(false);
            if (Comparer<T>.Default.Compare(item, default!) == 0)
                throw new InvalidOperationException("SelectFirst request has not returned a row.");

            return item;
        }

        /// <summary>
        /// Selects first item or default.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <returns>The single item or default.</returns>
        public async Task<T> SelectFirstOrDefaultAsync<T>(IDatabaseMapper<T> mapper) => await SelectSingleFirstAsync(mapper, false).ConfigureAwait(false);

        private async Task<T> SelectSingleFirstAsync<T>(IDatabaseMapper<T> mapper, bool throwWhereMulti)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            return await ExecuteWrapper(async () =>
            {
                T item = default!;
                int i = 0;

                using var cmd = await CreateDbCommandAsync().ConfigureAwait(false);
                using var dr = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

                while (await dr.ReadAsync().ConfigureAwait(false))
                {
                    if (i++ > 0)
                    {
                        if (throwWhereMulti)
                            throw new InvalidOperationException("SelectSingle request has returned more than one row.");

                        return item;
                    }

                    item = mapper.MapFromDb(new DatabaseRecord(dr));
                }

                return item;

            }).ConfigureAwait(false);
        }
    }
}