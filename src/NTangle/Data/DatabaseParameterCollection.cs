// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Data.Common;

namespace NTangle.Data
{
    /// <summary>
    /// Encapsulates the <see cref="DbParameter"/> creation for a <see cref="DbCommand"/>.
    /// </summary>
    public sealed class DatabaseParameterCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseParameterCollection"/>.
        /// </summary>
        /// <param name="command"></param>
        public DatabaseParameterCollection(DbCommand command) => Command = command ?? throw new ArgumentNullException(nameof(command)); 

        /// <summary>
        /// Gets the underlying <see cref="DbCommand"/>.
        /// </summary>
        public DbCommand Command { get; }

        /// <summary>
        /// Adds the named parameter and value, using the specified <see cref="DbType"/> and <see cref="ParameterDirection"/>, to the <see cref="DbCommand.Parameters"/>.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="dbType">The parameter <see cref="DbType"/>.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>A <see cref="DbParameter"/>.</returns>
        public DbParameter AddParameter(string name, object? value, DbType? dbType = null, ParameterDirection direction = ParameterDirection.Input)
        {
            var p = Command.CreateParameter();
            p.ParameterName = name ?? throw new ArgumentNullException(nameof(name));
            if (dbType.HasValue)
                p.DbType = dbType.Value;

            p.Value = value;
            p.Direction = direction;

            Command.Parameters.Add(p);
            return p;
        }

        /// <summary>
        /// Adds the named parameter and <see cref="TableValuedParameter"/> value to the <see cref="DbCommand.Parameters"/>.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="tvp">The <see cref="TableValuedParameter"/>.</param>
        /// <returns>A <see cref="DbParameter"/>.</returns>
        /// <remarks>This specifically implies that the <see cref="SqlParameter"/> is being used; if not then an exception will be thrown.</remarks>
        public DbParameter AddTableValuedParameter(string name, TableValuedParameter tvp)
        {
            var p = (SqlParameter)Command.CreateParameter();
            p.ParameterName = name ?? throw new ArgumentNullException(nameof(name));
            p.SqlDbType = SqlDbType.Structured;
            p.TypeName = tvp.TypeName;
            p.Value = tvp.Value;
            p.Direction = ParameterDirection.Input;

            Command.Parameters.Add(p);
            return p;
        }
    }
}