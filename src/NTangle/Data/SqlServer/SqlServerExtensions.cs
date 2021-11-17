// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Data.Common;

namespace NTangle.Data
{
    /// <summary>
    /// Provides <see cref="SqlServerDatabase"/> extension methods.
    /// </summary>
    public static class SqlServerExtensions
    {

        /// <summary>
        /// Adds the named parameter and <see cref="TableValuedParameter"/> value to the <see cref="DbCommand.Parameters"/>.
        /// </summary>
        /// <param name="dpc">The <see cref="DatabaseParameterCollection"/>.</param>
        /// <param name="name">The parameter name.</param>
        /// <param name="tvp">The <see cref="TableValuedParameter"/>.</param>
        /// <returns>A <see cref="DbParameter"/>.</returns>
        /// <remarks>This specifically implies that the <see cref="SqlParameter"/> is being used; if not then an exception will be thrown.</remarks>
        public static SqlParameter AddTableValuedParameter(this DatabaseParameterCollection dpc, string name, TableValuedParameter tvp)
        {
            var p = (SqlParameter)(dpc ?? throw new ArgumentNullException(nameof(dpc))).Command.CreateParameter();
            p.ParameterName = name ?? throw new ArgumentNullException(nameof(name));
            p.SqlDbType = SqlDbType.Structured;
            p.TypeName = tvp.TypeName;
            p.Value = tvp.Value;
            p.Direction = ParameterDirection.Input;

            dpc.Command.Parameters.Add(p);
            return p;
        }
    }
}