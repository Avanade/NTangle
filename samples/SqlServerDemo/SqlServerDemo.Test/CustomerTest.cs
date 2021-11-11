﻿using NTangle.Test;
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
                "DELETE FROM [Legacy].[Customer]" + Environment.NewLine +
                "INSERT INTO [Legacy].[Customer] ([CustomerId], [Name], [Email]) VALUES (1, 'Bob', 'bob@email.com')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Customer] ([CustomerId], [Name], [Email]) VALUES (2, 'Jane', 'jane@email.com')";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Reset our view of CDC as we do not want to include in the next data capture.
            script =
                "DELETE FROM [NTangle].[IdentifierMapping]" + Environment.NewLine +
                "DELETE FROM [NTangle].[VersionTracking]" + Environment.NewLine +
                "DELETE FROM [NTangle].[CustomerBatchTracking]" + Environment.NewLine +
                "DECLARE @Lsn BINARY(10)" + Environment.NewLine +
                "SET @Lsn = sys.fn_cdc_get_max_lsn()" + Environment.NewLine +
                "INSERT INTO [NTangle].[CustomerBatchTracking] ([CreatedDate], [CustomerMinLsn], [CustomerMaxLsn], [IsComplete], [CompletedDate], [HasDataLoss]) VALUES('2021-01-01T00:00:00', @Lsn, @Lsn, 1, '2021-01-01T00:00:00', 0)";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
        }

        [Test]
        public async Task LogicalDelete()
        {
            using var db = SqlServerUnitTest.GetDatabase();
            var logger = UnitTest.GetLogger<CustomerCdcOrchestrator>();

            // Update customer 1 and deleted customer 2.
            var script =
                "UPDATE [Legacy].[Customer] SET [is-deleted] = 1 WHERE [CustomerId] = 2" + Environment.NewLine +
                "UPDATE [Legacy].[Customer] SET [Email] = 'bob@domain.com' WHERE [CustomerId] = 1";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute should pick up the update and delete.
            var tep = new TestEventPublisher();
            var cdc = new CustomerCdcOrchestrator(db, tep, logger);
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, tep);

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
            Assert.AreEqual(2, tep.Events.Count);

            UnitTest.AssertEvent("CustomerTest-LogicalDelete-1.txt", tep.Events[0]);
            UnitTest.AssertEvent("CustomerTest-LogicalDelete-2.txt", tep.Events[1], "data.rowVersion");
        }

        [Test]
        public async Task ExcludeRowVersionFromETag()
        {
            using var db = SqlServerUnitTest.GetDatabase();
            var logger = UnitTest.GetLogger<CustomerCdcOrchestrator>();

            // Update customer 1.
            var script = "UPDATE [Legacy].[Customer] SET [Email] = 'bob@domain.com' WHERE [CustomerId] = 1";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute should pick up the update.
            var tep = new TestEventPublisher();
            var cdc = new CustomerCdcOrchestrator(db, tep, logger);
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, tep);

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
            Assert.AreEqual(1, tep.Events.Count);

            // Update excluded property; should not pick up even though RowVersion column would have been updated as version (ETag) excludes.
            script = "UPDATE [Legacy].[Customer] SET [internal-secret] = 'shhh' WHERE [CustomerId] = 1";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute should pick up the update.
            tep.Events.Clear();
            cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, tep);

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
            Assert.AreEqual(0, tep.Events.Count);
        }

        [Test]
        public async Task MaxQuerySize()
        {
            using var db = SqlServerUnitTest.GetDatabase();
            var logger = UnitTest.GetLogger<CustomerCdcOrchestrator>();

            var script =
                "INSERT INTO [Legacy].[Customer] ([CustomerId], [Name], [Email]) VALUES (11, 'eleven', 'eleven@email.com')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Customer] ([CustomerId], [Name], [Email]) VALUES (12, 'twelve', 'twelve@email.com')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Customer] ([CustomerId], [Name], [Email]) VALUES (13, 'thirteen', 'thirteen@email.com')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Customer] ([CustomerId], [Name], [Email]) VALUES (14, 'fourteen', 'fourteen@email.com')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Customer] ([CustomerId], [Name], [Email]) VALUES (15, 'fifteen', 'fifteen@email.com')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Customer] ([CustomerId], [Name], [Email]) VALUES (16, 'sixteen', 'sixteen@email.com')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Customer] ([CustomerId], [Name], [Email]) VALUES (17, 'seventeen', 'seventeen@email.com')";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute should pick up the first 3.
            var tep = new TestEventPublisher();
            var cdc = new CustomerCdcOrchestrator(db, tep, logger) { MaxQuerySize = 3 };
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, tep);

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
            Assert.AreEqual(3, tep.Events.Count);

            // Next Execute should pick up the next 3.
            tep.Events.Clear();
            cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, tep);

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
            Assert.AreEqual(3, tep.Events.Count);

            // Next Execute should pick up the last 1.
            tep.Events.Clear();
            cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, tep);

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
            Assert.AreEqual(1, tep.Events.Count);
        }
    }
}