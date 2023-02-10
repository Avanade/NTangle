using CoreEx.Database;
using CoreEx.Database.SqlServer;
using DbEx;
using DbEx.SqlServer.Console;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NTangle.Test;
using NUnit.Framework;
using SqlServerDemo.Database;
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
            return new SqlServerDatabase(() => new SqlConnection(cs));
        }

        /// <summary>
        /// Use the database project to create and setup the db.
        /// </summary>
        [OneTimeSetUp]
        public static async Task SetUpDatabase()
        {
            var result = await SqlServerMigrationConsole
                .Create<Program>(UnitTest.GetConfig("SqlServerDemo_").GetConnectionString("SqlDb"))
                .Assembly<Program>()
                .AcceptsPrompts()
                .RunAsync(MigrationCommand.DropAndAll).ConfigureAwait(false);

            if (result != 0)
                Assert.Fail("Database migration failed.");
        }
    }
}