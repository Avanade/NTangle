using CoreEx.Configuration;
using CoreEx.Events;
using CoreEx.Json;
using NTangle.Test;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SqlServerDemo.Publisher.Data;
using System.Threading.Tasks;

namespace SqlServerDemo.Test
{
    [TestFixture]
    [NonParallelizable]
    public class AAACustomerTest
    {
        [Test]
        public async Task InitialNoChanges()
        {
            // NOTE: This is intended for execution directly after the database has been created and no CDC capture has occured on the underlying table.

            using var db = SqlServerUnitTest.GetDatabase();
            var logger = UnitTest.GetLogger<CustomerOrchestrator>();

            // Execute should pick up the update and delete.
            var imp = new InMemoryPublisher(logger);
            var cdc = new CustomerOrchestrator(db, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger);
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            ClassicAssert.NotNull(cdcr);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNull(cdcr.BatchTracker);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(0, cdcr.ExecuteStatus.InitialCount);
            ClassicAssert.IsNull(cdcr.ExecuteStatus.ConsolidatedCount);
            ClassicAssert.IsNull(cdcr.ExecuteStatus.PublishCount);
            ClassicAssert.AreEqual(0, imp.GetEvents().Length);
        }
    }
}