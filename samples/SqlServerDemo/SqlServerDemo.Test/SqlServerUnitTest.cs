using DbEx;
using DbEx.Console;
using DbEx.Migration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NTangle.Data.SqlServer;
using NTangle.Test;
using NUnit.Framework;
using SqlServerDemo.Database;
using System;
using System.Threading.Tasks;

namespace SqlServerDemo.Test
{
    /// <summary>
    /// Provides test helping/utility functions.
    /// </summary>
    [SetUpFixture]
    public class SqlServerUnitTest
    {
        /// <summary>
        /// Gets the <see cref="SqlServerDatabase"/> used for testing.
        /// </summary>
        /// <returns>The <see cref="SqlServerDatabase"/>.</returns>
        public static IDatabase GetDatabase()
        {
            var cs = UnitTest.GetConfig("SqlServerDemo_").GetConnectionString("SqlDb");
            Console.WriteLine($"ConnectionString: {cs}");
            return new SqlServerDatabase(() => new SqlConnection(cs));
        }

        /// <summary>
        /// Use the database project to create and setup the db.
        /// </summary>
        [OneTimeSetUp]
        public static async Task SetUpDatabase()
            => await SqlServerMigratorConsole
                .Create<Program>(UnitTest.GetConfig("SqlServerDemo_").GetConnectionString("SqlDb"))
                .ConsoleArgs(a => a.AddAssembly(typeof(Program).Assembly))
                .RunAsync(MigrationCommand.DropAndAll).ConfigureAwait(false);
    }
}