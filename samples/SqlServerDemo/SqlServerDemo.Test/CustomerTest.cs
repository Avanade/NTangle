using CoreEx.Events;
using CoreEx.Json;
using NTangle.Test;
using NUnit.Framework;
using SqlServerDemo.Publisher.Data;
using System;
using System.Threading.Tasks;

namespace SqlServerDemo.Test
{
    [TestFixture]
    [NonParallelizable]
    public class CustomerTest
    {
        [SetUp]
        public async Task InitTest()
        {
            using var db = SqlServerUnitTest.GetDatabase();

            // Create some data.
            var script =
                "DELETE FROM [Legacy].[Cust]" + Environment.NewLine +
                "INSERT INTO [Legacy].[Cust] ([CustId], [Name], [Email]) VALUES (1, 'Bob', 'bob@email.com')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Cust] ([CustId], [Name], [Email], [is-private]) VALUES (99, 'Agent', 'ninety-nine@email.com', 1)" + Environment.NewLine +
                "INSERT INTO [Legacy].[Cust] ([CustId], [Name], [Email]) VALUES (2, 'Jane', 'jane@email.com')";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Reset our view of CDC as we do not want to include in the next data capture.
            script =
                "DELETE FROM [NTangle].[IdentifierMapping]" + Environment.NewLine +
                "DELETE FROM [NTangle].[VersionTracking]" + Environment.NewLine +
                "DELETE FROM [NTangle].[CustomerBatchTracking]" + Environment.NewLine +
                "EXEC [NTangle].[spCustomerBatchReset]";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
        }

        [Test]
        public async Task LogicalDelete()
        {
            using var db = SqlServerUnitTest.GetDatabase();
            var logger = UnitTest.GetLogger<CustomerOrchestrator>();

            // Update customer 1 and deleted customer 2.
            var script =
                "UPDATE [Legacy].[Cust] SET [is-deleted] = 1 WHERE [CustId] = 2" + Environment.NewLine +
                "UPDATE [Legacy].[Cust] SET [Email] = 'bob@domain.com' WHERE [CustId] = 1";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute should pick up the update and delete.
            var imp = new InMemoryPublisher(logger);
            var cdc = new CustomerOrchestrator(db, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger);
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            Assert.NotNull(cdcr);
            Assert.IsTrue(cdcr.IsSuccessful);
            Assert.IsNotNull(cdcr.Batch);
            Assert.IsTrue(cdcr.Batch.IsComplete);
            Assert.IsNotNull(cdcr.Batch.CompletedDate);
            Assert.IsNotNull(cdcr.Batch.CorrelationId);
            Assert.IsFalse(cdcr.Batch.HasDataLoss);
            Assert.IsNull(cdcr.Exception);
            Assert.AreEqual(2, cdcr.ExecuteStatus?.InitialCount);
            Assert.AreEqual(2, cdcr.ExecuteStatus?.ConsolidatedCount);
            Assert.AreEqual(2, cdcr.ExecuteStatus?.PublishCount);

            var events = imp.GetEvents();
            Assert.AreEqual(2, events.Length);

            UnitTest.AssertEvent("CustomerTest-LogicalDelete-1.txt", events[0]);
            UnitTest.AssertEvent("CustomerTest-LogicalDelete-2.txt", events[1], "value.rowVersion");
        }

        [Test]
        public async Task ExcludeRowVersionFromETag()
        {
            using var db = SqlServerUnitTest.GetDatabase();
            var logger = UnitTest.GetLogger<CustomerOrchestrator>();

            // Update customer 1.
            var script = "UPDATE [Legacy].[Cust] SET [Email] = 'bob@domain.com' WHERE [CustId] = 1";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute should pick up the update.
            var imp = new InMemoryPublisher(logger);
            var cdc = new CustomerOrchestrator(db, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger);
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            Assert.NotNull(cdcr);
            Assert.IsTrue(cdcr.IsSuccessful);
            Assert.IsNotNull(cdcr.Batch);
            Assert.IsTrue(cdcr.Batch.IsComplete);
            Assert.IsNotNull(cdcr.Batch.CompletedDate);
            Assert.IsNotNull(cdcr.Batch.CorrelationId);
            Assert.IsFalse(cdcr.Batch.HasDataLoss);
            Assert.IsNull(cdcr.Exception);
            Assert.AreEqual(1, cdcr.ExecuteStatus?.InitialCount);
            Assert.AreEqual(1, cdcr.ExecuteStatus?.ConsolidatedCount);
            Assert.AreEqual(1, cdcr.ExecuteStatus?.PublishCount);

            var events = imp.GetEvents();
            Assert.AreEqual(1, events.Length);

            // Update excluded property; should not pick up even though RowVersion column would have been updated as version (ETag) excludes.
            script = "UPDATE [Legacy].[Cust] SET [internal-secret] = 'shhh' WHERE [CustId] = 1";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute should pick up the update.
            imp.Reset();
            cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            Assert.NotNull(cdcr);
            Assert.IsTrue(cdcr.IsSuccessful);
            Assert.IsNotNull(cdcr.Batch);
            Assert.IsTrue(cdcr.Batch.IsComplete);
            Assert.IsNotNull(cdcr.Batch.CompletedDate);
            Assert.IsNotNull(cdcr.Batch.CorrelationId);
            Assert.IsFalse(cdcr.Batch.HasDataLoss);
            Assert.IsNull(cdcr.Exception);
            Assert.AreEqual(1, cdcr.ExecuteStatus?.InitialCount);
            Assert.AreEqual(1, cdcr.ExecuteStatus?.ConsolidatedCount);
            Assert.AreEqual(0, cdcr.ExecuteStatus?.PublishCount);
            Assert.AreEqual(0, imp.GetEvents().Length);
        }

        [Test]
        public async Task MaxQuerySize()
        {
            using var db = SqlServerUnitTest.GetDatabase();
            var logger = UnitTest.GetLogger<CustomerOrchestrator>();

            var script =
                "INSERT INTO [Legacy].[Cust] ([CustId], [Name], [Email]) VALUES (11, 'eleven', 'eleven@email.com')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Cust] ([CustId], [Name], [Email]) VALUES (12, 'twelve', 'twelve@email.com')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Cust] ([CustId], [Name], [Email]) VALUES (13, 'thirteen', 'thirteen@email.com')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Cust] ([CustId], [Name], [Email]) VALUES (14, 'fourteen', 'fourteen@email.com')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Cust] ([CustId], [Name], [Email]) VALUES (15, 'fifteen', 'fifteen@email.com')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Cust] ([CustId], [Name], [Email]) VALUES (16, 'sixteen', 'sixteen@email.com')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Cust] ([CustId], [Name], [Email]) VALUES (17, 'seventeen', 'seventeen@email.com')";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute should pick up the first 3.
            var imp = new InMemoryPublisher(logger);
            var cdc = new CustomerOrchestrator(db, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger) { MaxQuerySize = 3 };
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            Assert.NotNull(cdcr);
            Assert.IsTrue(cdcr.IsSuccessful);
            Assert.IsNotNull(cdcr.Batch);
            Assert.IsTrue(cdcr.Batch.IsComplete);
            Assert.IsNotNull(cdcr.Batch.CompletedDate);
            Assert.IsNotNull(cdcr.Batch.CorrelationId);
            Assert.IsFalse(cdcr.Batch.HasDataLoss);
            Assert.IsNull(cdcr.Exception);
            Assert.AreEqual(3, cdcr.ExecuteStatus?.InitialCount);
            Assert.AreEqual(3, cdcr.ExecuteStatus?.ConsolidatedCount);
            Assert.AreEqual(3, cdcr.ExecuteStatus?.PublishCount);
            Assert.AreEqual(3, imp.GetEvents().Length);

            // Next Execute should pick up the next 3.
            imp.Reset();
            cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            Assert.NotNull(cdcr);
            Assert.IsTrue(cdcr.IsSuccessful);
            Assert.IsNotNull(cdcr.Batch);
            Assert.IsTrue(cdcr.Batch.IsComplete);
            Assert.IsNotNull(cdcr.Batch.CompletedDate);
            Assert.IsNotNull(cdcr.Batch.CorrelationId);
            Assert.IsFalse(cdcr.Batch.HasDataLoss);
            Assert.IsNull(cdcr.Exception);
            Assert.AreEqual(3, cdcr.ExecuteStatus?.InitialCount);
            Assert.AreEqual(3, cdcr.ExecuteStatus?.ConsolidatedCount);
            Assert.AreEqual(3, cdcr.ExecuteStatus?.PublishCount);
            Assert.AreEqual(3, imp.GetEvents().Length);

            // Next Execute should pick up the last 1.
            imp.Reset();
            cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            Assert.NotNull(cdcr);
            Assert.IsTrue(cdcr.IsSuccessful);
            Assert.IsNotNull(cdcr.Batch);
            Assert.IsTrue(cdcr.Batch.IsComplete);
            Assert.IsNotNull(cdcr.Batch.CompletedDate);
            Assert.IsNotNull(cdcr.Batch.CorrelationId);
            Assert.IsFalse(cdcr.Batch.HasDataLoss);
            Assert.IsNull(cdcr.Exception);
            Assert.AreEqual(1, cdcr.ExecuteStatus?.InitialCount);
            Assert.AreEqual(1, cdcr.ExecuteStatus?.ConsolidatedCount);
            Assert.AreEqual(1, cdcr.ExecuteStatus?.PublishCount);
            Assert.AreEqual(1, imp.GetEvents().Length);
        }
    }
}