using NTangle.Data;
using NTangle.Test;
using NUnit.Framework;
using SqlServerDemo.Publisher.Data;
using System;
using System.Threading.Tasks;

namespace SqlServerDemo.Test
{
    [TestFixture]
    [NonParallelizable]
    public class PostsTest
    {
        [SetUp]
        public async Task InitTest()
        {
            using var db = SqlServerUnitTest.GetDatabase();

            // Create some data.
            var script =
                "DELETE FROM [Legacy].[Posts]" + Environment.NewLine +
                "DELETE FROM [Legacy].[Comments]" + Environment.NewLine +
                "DELETE FROM [Legacy].[Tags]" + Environment.NewLine +
                "INSERT INTO [Legacy].[Posts] ([PostsId], [Text], [Date]) VALUES (1, 'Blah 1', '2020-01-01T15:30:42')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Posts] ([PostsId], [Text], [Date]) VALUES (2, 'Blah 2', '2020-01-02T15:30:42')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Posts] ([PostsId], [Text], [Date]) VALUES (3, 'Blah 3', '2020-01-03T15:30:42')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Comments] ([CommentsId], [PostsId], [Text], [Date]) VALUES (101, 1, 'Blah blah 101', '2020-01-01T18:30:42')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Comments] ([CommentsId], [PostsId], [Text], [Date]) VALUES (102, 1, 'Blah blah 102', '2020-01-01T19:30:42')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Comments] ([CommentsId], [PostsId], [Text], [Date]) VALUES (301, 3, 'Blah blah 301', '2020-01-03T18:30:42')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Tags] ([TagsId], [ParentType], [ParentId], [Text]) VALUES (1001, 'P', 1, '#Blah1')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Tags] ([TagsId], [ParentType], [ParentId], [Text]) VALUES (2001, 'P', 2, '#Blah2')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Tags] ([TagsId], [ParentType], [ParentId], [Text]) VALUES (2002, 'P', 2, '#Tag')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Tags] ([TagsId], [ParentType], [ParentId], [Text]) VALUES (3301, 'C', 301, '#Tag')";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Reset our view of CDC as we do not want to include in the next data capture.
            script =
                "DELETE FROM [NTangle].[VersionTracking]" + Environment.NewLine +
                "DELETE FROM [NTangle].[PostsBatchTracking]" + Environment.NewLine +
                "DECLARE @Lsn BINARY(10)" + Environment.NewLine +
                "SET @Lsn = sys.fn_cdc_get_max_lsn()" + Environment.NewLine +
                "INSERT INTO [NTangle].[PostsBatchTracking] ([CreatedDate], [PostsMinLsn], [PostsMaxLsn], [CommentsMinLsn], [CommentsMaxLsn], [CommentsTagsMinLsn], [CommentsTagsMaxLsn], [PostsTagsMinLsn], [PostsTagsMaxLsn], [IsComplete], [CompletedDate], [HasDataLoss]) VALUES('2021-01-01T00:00:00', @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, 1, '2021-01-01T00:00:00', 0)";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
        }

        [Test]
        public async Task InvalidBatchStatus()
        {
            using var db = SqlServerUnitTest.GetDatabase();
            var logger = UnitTest.GetLogger<PostCdcOrchestrator>();

            // Add two incomplete batches.
            var script =
                "DECLARE @Lsn BINARY(10)" + Environment.NewLine +
                "SET @Lsn = sys.fn_cdc_get_max_lsn()" + Environment.NewLine +
                "INSERT INTO [NTangle].[PostsBatchTracking] ([CreatedDate], [PostsMinLsn], [PostsMaxLsn], [CommentsMinLsn], [CommentsMaxLsn], [CommentsTagsMinLsn], [CommentsTagsMaxLsn], [PostsTagsMinLsn], [PostsTagsMaxLsn], [IsComplete], [CompletedDate], [HasDataLoss]) VALUES('2021-01-01T00:00:00', @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, 0, '2021-01-01T00:00:00', 0)" + Environment.NewLine +
                "INSERT INTO [NTangle].[PostsBatchTracking] ([CreatedDate], [PostsMinLsn], [PostsMaxLsn], [CommentsMinLsn], [CommentsMaxLsn], [CommentsTagsMinLsn], [CommentsTagsMaxLsn], [PostsTagsMinLsn], [PostsTagsMaxLsn], [IsComplete], [CompletedDate], [HasDataLoss]) VALUES('2021-01-01T00:00:00', @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, 0, '2021-01-01T00:00:00', 0)";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);

            var tep = new TestEventPublisher();
            var cdc = new PostCdcOrchestrator(db, tep, logger);
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, tep);

            // Assert/verify the results.
            Assert.NotNull(cdcr);
            Assert.IsFalse(cdcr.IsSuccessful);
            Assert.IsNull(cdcr.Batch);
            Assert.NotNull(cdcr.Exception);
            Assert.IsInstanceOf<DatabaseErrorException>(cdcr.Exception);
            Assert.AreEqual("There are multiple incomplete batches; there should not be more than one incomplete batch at any one time.", cdcr.Exception.Message);
        }

        [Test]
        public async Task NoChanges()
        {
            using var db = SqlServerUnitTest.GetDatabase();
            var logger = UnitTest.GetLogger<PostCdcOrchestrator>();

            var tep = new TestEventPublisher();
            var cdc = new PostCdcOrchestrator(db, tep, logger);
            await UnitTest.AssertNoFurtherChanges(cdc, tep).ConfigureAwait(false);
        }

        [Test]
        public async Task MultipleChanges()
        {
            using var db = SqlServerUnitTest.GetDatabase();
            var logger = UnitTest.GetLogger<PostCdcOrchestrator>();

            // Make some table changes for cdc tracking.
            var script =
                "UPDATE [Legacy].[Comments] SET [Text] = 'Blah blah 101 some more' WHERE [CommentsId] = 101" + Environment.NewLine +
                "DELETE FROM [Legacy].[Tags] WHERE [TagsId] = 3301" + Environment.NewLine +
                "INSERT INTO [Legacy].[Posts] ([PostsId], [Text], [Date]) VALUES (4, 'Blah 4', '2020-01-01T15:30:42')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Comments] ([CommentsId], [PostsId], [Text], [Date]) VALUES (401, 4, 'Blah blah 401', '2020-01-01T18:30:42')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Tags] ([TagsId], [ParentType], [ParentId], [Text]) VALUES (1002, 'C', 101, '#groovy')";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute picking up the changes.
            var tep = new TestEventPublisher();
            var cdc = new PostCdcOrchestrator(db, tep, logger);
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
            Assert.AreEqual(3, tep.Events.Count);

            UnitTest.AssertEvent("PostsTest-MultipleChanges-1.txt", tep.Events[0]);
            UnitTest.AssertEvent("PostsTest-MultipleChanges-2.txt", tep.Events[1]);
            UnitTest.AssertEvent("PostsTest-MultipleChanges-3.txt", tep.Events[2]);

            // Ensure procesed correctly, execute again with no changes.
            await UnitTest.AssertNoFurtherChanges(cdc, tep).ConfigureAwait(false);
        }

        [Test]
        public async Task UpdateThenDelete()
        {
            using var db = SqlServerUnitTest.GetDatabase();
            var logger = UnitTest.GetLogger<PostCdcOrchestrator>();

            // Update a row and then delete - should only result in a delete.
            var script =
                "UPDATE [Legacy].[Comments] SET [Text] = 'Bananas' WHERE [CommentsId] = 101" + Environment.NewLine +
                "DELETE FROM [Legacy].[Posts] WHERE [PostsId] = 1";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute picking up the _delete_ only.
            var tep = new TestEventPublisher();
            var cdc = new PostCdcOrchestrator(db, tep, logger);
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
            Assert.AreEqual(1, tep.Events.Count);

            UnitTest.AssertEvent("PostsTest-UpdateThenDelete.txt", tep.Events[0]);

            // Ensure procesed correctly, execute again with no changes.
            await UnitTest.AssertNoFurtherChanges(cdc, tep).ConfigureAwait(false);
        }

        [Test]
        public async Task UpdateSameVersion()
        {
            using var db = SqlServerUnitTest.GetDatabase();
            var logger = UnitTest.GetLogger<PostCdcOrchestrator>();

            var script =
                "UPDATE [Legacy].[Tags] SET [TEXT] = '#Other' WHERE [TagsId] = 3301" + Environment.NewLine +
                "UPDATE [Legacy].[Tags] SET [TEXT] = '#Tag' WHERE [TagsId] = 3301";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute picking up the changes resulting in a new event and corresponding version record.
            var tep = new TestEventPublisher();
            var cdc = new PostCdcOrchestrator(db, tep, logger);
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
            Assert.AreEqual(1, tep.Events.Count);

            // Replay the database changes; i.e. change and then change back to original.
            script =
                "UPDATE [Legacy].[Tags] SET [TEXT] = '#Other' WHERE [TagsId] = 3301" + Environment.NewLine +
                "UPDATE [Legacy].[Tags] SET [TEXT] = '#Tag' WHERE [TagsId] = 3301";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute picking up the changes but should have no event due to same version.
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
            Assert.AreEqual(0, tep.Events.Count);

            // Ensure procesed correctly, execute again with no changes.
            await UnitTest.AssertNoFurtherChanges(cdc, tep).ConfigureAwait(false);
        }

        [Test]
        public async Task DataLossStop()
        {
            using var db = SqlServerUnitTest.GetDatabase();
            var logger = UnitTest.GetLogger<PostCdcOrchestrator>();

            // Set up cdc tracking for data too far in the past; then update a Post to trigger CDC.
            var script =
                "DECLARE @extraction_time datetime, @lsn binary(10)" + Environment.NewLine +
                "SET @extraction_time = '1999-12-15 12:00:00.000'" + Environment.NewLine +
                "SELECT @lsn = sys.fn_cdc_map_time_to_lsn('smallest greater than or equal', @extraction_time)" + Environment.NewLine +
                "UPDATE [NTangle].[PostsBatchTracking] SET PostsMinLsn = 0x00000000000000000001, PostsMaxLsn = 0x00000000000000000001" + Environment.NewLine +
                "UPDATE [Legacy].[Comments] SET [Text] = 'Bananas' WHERE [CommentsId] = 101";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute picking up the changes _but_ discovering a data loss situation.
            var tep = new TestEventPublisher();
            var cdc = new PostCdcOrchestrator(db, tep, logger);
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, tep);

            // Assert/verify the results.
            Assert.NotNull(cdcr);
            Assert.IsFalse(cdcr.IsSuccessful);
            Assert.IsNull(cdcr.Batch);
            Assert.NotNull(cdcr.Exception);
            Assert.IsInstanceOf<DatabaseErrorException>(cdcr.Exception);
            Assert.AreEqual("Unexpected data loss error for 'Legacy.Posts'; this indicates that the CDC data has probably been cleaned up before being successfully processed.", cdcr.Exception.Message);
        }

        [Test]
        public async Task DataLossContinue()
        {
            using var db = SqlServerUnitTest.GetDatabase();
            var logger = UnitTest.GetLogger<PostCdcOrchestrator>();

            // Set up cdc tracking for data too far in the past; then update a Post to trigger CDC.
            var script =
                "DECLARE @extraction_time datetime, @lsn binary(10)" + Environment.NewLine +
                "SET @extraction_time = '1999-12-15 12:00:00.000'" + Environment.NewLine +
                "SELECT @lsn = sys.fn_cdc_map_time_to_lsn('smallest greater than or equal', @extraction_time)" + Environment.NewLine +
                "UPDATE [NTangle].[PostsBatchTracking] SET PostsMinLsn = 0x00000000000000000001, PostsMaxLsn = 0x00000000000000000001" + Environment.NewLine +
                "UPDATE [Legacy].[Comments] SET [Text] = 'Bananas' WHERE [CommentsId] = 101";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute picking up the changes contuning with the data loss situation.
            var tep = new TestEventPublisher();
            var cdc = new PostCdcOrchestrator(db, tep, logger) { ContinueWithDataLoss = true };
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, tep);

            // Assert/verify the results.
            Assert.NotNull(cdcr);
            Assert.IsTrue(cdcr.IsSuccessful);
            Assert.IsNotNull(cdcr.Batch);
            Assert.IsTrue(cdcr.Batch.IsComplete);
            Assert.IsNotNull(cdcr.Batch.CompletedDate);
            Assert.IsNotNull(cdcr.Batch.CorrelationId);
            Assert.IsTrue(cdcr.Batch.HasDataLoss);
            Assert.IsNull(cdcr.Exception);
        }

        [Test]
        public async Task RetryWhenIncomplete()
        {
            using var db = SqlServerUnitTest.GetDatabase();
            var logger = UnitTest.GetLogger<PostCdcOrchestrator>();

            var script = "UPDATE [Legacy].[Tags] SET [TEXT] = '#Other' WHERE [TagsId] = 3301";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute picking up the changes.
            var tep = new TestEventPublisher();
            var cdc = new PostCdcOrchestrator(db, tep, logger);
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
            Assert.AreEqual(1, tep.Events.Count);

            // Set the last batch to incomplete and delete version tracking so i will publish event again - simulate batch failure.
            script =
                $"UPDATE [NTangle].[PostsBatchTracking] SET IsComplete = 0 WHERE CorrelationId = '{cdcr.Batch.CorrelationId}'" + Environment.NewLine +
                "DELETE FROM [NTangle].[VersionTracking]";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);

            // Execute picking up the existing batch and re-completing.
            tep.Events.Clear();
            var cdcr2 = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr2, tep);

            // Assert/verify the results.
            Assert.NotNull(cdcr2);
            Assert.IsTrue(cdcr2.IsSuccessful);
            Assert.IsNotNull(cdcr2.Batch);
            Assert.IsTrue(cdcr2.Batch.IsComplete);
            Assert.IsNotNull(cdcr2.Batch.CompletedDate);
            Assert.IsNotNull(cdcr2.Batch.CorrelationId);
            Assert.IsFalse(cdcr2.Batch.HasDataLoss);
            Assert.IsNull(cdcr2.Exception);
            Assert.AreEqual(cdcr.Batch.Id, cdcr2.Batch.Id);
            Assert.AreEqual(cdcr.Batch.CorrelationId, cdcr2.Batch.CorrelationId);
            Assert.AreNotEqual(cdcr.ExecutionId, cdcr2.ExecutionId);
            Assert.AreEqual(1, tep.Events.Count);
        }
    }
}