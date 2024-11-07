using CoreEx.Database;
using CoreEx.Database.SqlServer;
using DbEx;
using DbEx.SqlServer.Console;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NTangle.Data;
using NTangle.Data.SqlServer;
using NTangle.Test;
using NUnit.Framework;
using SqlServerSidecarDemo.Database;
using System.Threading.Tasks;

namespace SqlServerSidecarDemo.Test
{
    /// <summary>
    /// Provides test helping/utility functions.
    /// </summary>
    [SetUpFixture]
    public class SqlServerSidecarUnitTest
    {
        /// <summary>
        /// Gets the <see cref="SqlServerDatabase"/> used for testing.
        /// </summary>
        /// <returns>The <see cref="SqlServerDatabase"/>.</returns>
        public static IDatabase GetDatabase()
        {
            var cs = UnitTest.GetConfig("SqlServerSidecarDemo_").GetConnectionString("SqlDb");
            return new SqlServerDatabase(() => new SqlConnection(cs));
        }

        /// <summary>
        /// Gets the sidecar <see cref="SqlServerDatabase"/> used for testing.
        /// </summary>
        /// <returns>The <see cref="SqlServerDatabase"/>.</returns>
        public static ISidecarDatabase GetSidecarDatabase()
        {
            var cs = UnitTest.GetConfig("SqlServerSidecarDemo_").GetConnectionString("SidecarDb");
            return new SqlServerSidecarDatabase(() => new SqlConnection(cs));
        }

        /// <summary>
        /// Gets the latest LSN.
        /// </summary>
        /// <param name="db">The primary database.</param>
        /// <returns>The LSN.</returns>
        public static async Task<byte[]> GetLatestLsnAsync(IDatabase db) => await db.SqlStatement("select sys.fn_cdc_get_max_lsn()").ScalarAsync<byte[]>();

        /// <summary>
        /// Resets the batch to the maximum LSN.
        /// </summary>
        /// <param name="db">The primary database.</param>
        /// <param name="sdb">The sidecar database.</param>
        public static async Task ResetBatchAsync(IDatabase db, ISidecarDatabase sdb, string sp)
        {
            var lsn = await GetLatestLsnAsync(db);
            await sdb.StoredProcedure(sp).Param("MaxLsn", lsn, System.Data.DbType.Binary).NonQueryAsync();
        }

        /// <summary>
        /// Use the database projects to create and setup the dbs.
        /// </summary>
        [OneTimeSetUp]
        public static async Task SetUpDatabase()
        {
            var result = await SqlServerMigrationConsole
                .Create<Database.Program>(UnitTest.GetConfig("SqlServerSidecarDemo_").GetConnectionString("SqlDb")!)
                .Assembly<Database.Program>()
                .AcceptsPrompts()
                .RunAsync(MigrationCommand.DropAndAll).ConfigureAwait(false);

            if (result != 0)
                Assert.Fail("Database migration failed.");

            result = await SqlServerMigrationConsole
                .Create<SidecarDb.Program> (UnitTest.GetConfig("SqlServerSidecarDemo_").GetConnectionString("SidecarDb")!)
                .Assembly<SidecarDb.Program>()
                .AcceptsPrompts()
                .RunAsync(MigrationCommand.DropAndAll).ConfigureAwait(false);

            if (result != 0)
                Assert.Fail("Sidecar Database migration failed.");
        }
    }
}