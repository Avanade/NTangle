// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Database;
using CoreEx.Database.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;

namespace NTangle.Data.SqlServer
{
    /// <summary>
    /// Provides <see href="https://learn.microsoft.com/en-us/sql/?view=sql-server-ver16">SQL Server</see> <see cref="ISidecarDatabase"/> access functionality.
    /// </summary>
    /// <param name="create">The function to create the <see cref="SqlConnection"/>.</param>
    /// <param name="logger">The optional <see cref="ILogger"/>.</param>
    /// <param name="invoker">The optional <see cref="DatabaseInvoker"/>.</param>
    public class SqlServerSidecarDatabase(Func<SqlConnection> create, ILogger<SqlServerDatabase>? logger = null, DatabaseInvoker? invoker = null) : SqlServerDatabase(create, logger, invoker), ISidecarDatabase { }
}