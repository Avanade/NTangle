// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

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
    }
}