using CoreEx.Configuration;
using CoreEx.Events;
using CoreEx.Json;
using NTangle.Test;
using NUnit.Framework;
using SqlServerSidecarDemo.Publisher.Data;
using System.Threading.Tasks;

namespace SqlServerSidecarDemo.Test
{
    [TestFixture]
    [NonParallelizable]
    public class AAACustomerTest
    {
        [Test]
        public async Task InitialNoChanges()
        {
            // NOTE: This is intended for execution directly after the database has been created and no CDC capture has occured on the underlying table.

            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<CustomerOrchestrator>();

            // Execute should pick up the update and delete.
            var imp = new InMemoryPublisher(logger);
            var cdc = new CustomerOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger);
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            Assert.NotNull(cdcr);
            Assert.IsTrue(cdcr.IsSuccessful);
            Assert.IsNull(cdcr.Batch);
            Assert.IsNull(cdcr.Exception);
            Assert.AreEqual(0, cdcr.ExecuteStatus!.InitialCount);
            Assert.IsNull(cdcr.ExecuteStatus.ConsolidatedCount);
            Assert.IsNull(cdcr.ExecuteStatus.PublishCount);
            Assert.AreEqual(0, imp.GetEvents().Length);
        }
    }
}