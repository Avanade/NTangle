using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NTangle.Data;
using System;

namespace SqlServerDemo.Test
{
    /// <summary>
    /// Provides test helping/utility functions.
    /// </summary>
    public static class SqlServerUnitTest
    {
        /// <summary>
        /// Gets the <see cref="SqlServerDatabase"/> used for testing.
        /// </summary>
        /// <returns>The <see cref="SqlServerDatabase"/>.</returns>
        public static IDatabase GetDatabase()
        {
            var cb = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
            var cs = cb.GetConnectionString("SqlDb");
            return new SqlServerDatabase(() => new SqlConnection(cs));
        }
    }
}