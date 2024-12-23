using CoreEx;
using CoreEx.Database;
using CoreEx.Events;
using CoreEx.Json;
using NTangle.Test;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SqlServerSidecarDemo.Publisher.Data;
using SqlServerSidecarDemo.Test;
using System;
using System.Threading.Tasks;

namespace SqlServerSidecarDemo.Test
{
    [TestFixture]
    [NonParallelizable]
    public class PostsTest
    {
        [SetUp]
        public async Task InitTest()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();

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
                "DELETE FROM [NTangle].[PostsBatchTracking]";

            await sdb.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);

            await SqlServerSidecarUnitTest.ResetBatchAsync(db, sdb, "[NTangle].[spPostsBatchReset]");
        }

        [Test]
        public async Task InvalidBatchStatus()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<PostOrchestrator>();

            // Add two incomplete batches.
            var lsn = await SqlServerSidecarUnitTest.GetLatestLsnAsync(db);

            var script =
                "INSERT INTO [NTangle].[PostsBatchTracking] ([CreatedDate], [PostsMinLsn], [PostsMaxLsn], [CommentsMinLsn], [CommentsMaxLsn], [CommentsTagsMinLsn], [CommentsTagsMaxLsn], [PostsTagsMinLsn], [PostsTagsMaxLsn], [IsComplete], [CompletedDate], [HasDataLoss]) VALUES('2021-01-01T00:00:00', @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, 0, '2021-01-01T00:00:00', 0)" + Environment.NewLine +
                "INSERT INTO [NTangle].[PostsBatchTracking] ([CreatedDate], [PostsMinLsn], [PostsMaxLsn], [CommentsMinLsn], [CommentsMaxLsn], [CommentsTagsMinLsn], [CommentsTagsMaxLsn], [PostsTagsMinLsn], [PostsTagsMaxLsn], [IsComplete], [CompletedDate], [HasDataLoss]) VALUES('2021-01-01T00:00:00', @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, 0, '2021-01-01T00:00:00', 0)";

            await sdb.SqlStatement(script).Param("Lsn", lsn, System.Data.DbType.Binary).NonQueryAsync().ConfigureAwait(false);

            var imp = new InMemoryPublisher(logger);
            var cdc = new PostOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger);
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            ClassicAssert.NotNull(cdcr);
            ClassicAssert.IsFalse(cdcr.IsSuccessful);
            ClassicAssert.IsNull(cdcr.BatchTracker);
            ClassicAssert.NotNull(cdcr.Exception);
            ClassicAssert.IsInstanceOf<DataConsistencyException>(cdcr.Exception);
            ClassicAssert.AreEqual("There are multiple incomplete batches; there should not be more than one incomplete batch at any one time.", cdcr.Exception!.Message);
            ClassicAssert.IsNull(cdcr.ExecuteStatus);
        }

        [Test]
        public async Task NoChanges()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<PostOrchestrator>();

            var imp = new InMemoryPublisher(logger);
            var cdc = new PostOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger);
            var cdcr = await UnitTest.AssertNoFurtherChanges(cdc, imp).ConfigureAwait(false);

            ClassicAssert.AreEqual(0, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.IsNull(cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.IsNull(cdcr.ExecuteStatus?.PublishCount);
        }

        [Test]
        public async Task MultipleChanges()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<PostOrchestrator>();

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
            var imp = new InMemoryPublisher(logger);
            var cdc = new PostOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger);
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            ClassicAssert.NotNull(cdcr);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNotNull(cdcr.BatchTracker);
            ClassicAssert.IsTrue(cdcr.BatchTracker!.IsComplete);
            ClassicAssert.IsNotNull(cdcr.BatchTracker.CompletedDate);
            ClassicAssert.IsNotNull(cdcr.BatchTracker.CorrelationId);
            ClassicAssert.IsFalse(cdcr.BatchTracker.HasDataLoss);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(3, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(3, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(3, cdcr.ExecuteStatus?.PublishCount);

            var events = imp.GetEvents();
            ClassicAssert.AreEqual(3, events.Length);

            UnitTest.AssertEvent("PostsTest-MultipleChanges-1.txt", events[0]);
            UnitTest.AssertEvent("PostsTest-MultipleChanges-2.txt", events[1]);
            UnitTest.AssertEvent("PostsTest-MultipleChanges-3.txt", events[2]);

            // Ensure procesed correctly, execute again with no changes.
            await UnitTest.AssertNoFurtherChanges(cdc, imp).ConfigureAwait(false);
        }

        [Test]
        public async Task UpdateThenDelete()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<PostOrchestrator>();

            // Update a row and then delete - should only result in a delete.
            var script =
                "UPDATE [Legacy].[Comments] SET [Text] = 'Bananas' WHERE [CommentsId] = 101" + Environment.NewLine +
                "DELETE FROM [Legacy].[Posts] WHERE [PostsId] = 1";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute picking up the _delete_ only.
            var imp = new InMemoryPublisher(logger);
            var cdc = new PostOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger);
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            ClassicAssert.NotNull(cdcr);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNotNull(cdcr.BatchTracker);
            ClassicAssert.IsTrue(cdcr.BatchTracker!.IsComplete);
            ClassicAssert.IsNotNull(cdcr.BatchTracker.CompletedDate);
            ClassicAssert.IsNotNull(cdcr.BatchTracker.CorrelationId);
            ClassicAssert.IsFalse(cdcr.BatchTracker.HasDataLoss);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.PublishCount);

            var events = imp.GetEvents();
            ClassicAssert.AreEqual(1, events.Length);

            UnitTest.AssertEvent("PostsTest-UpdateThenDelete.txt", events[0]);

            // Ensure procesed correctly, execute again with no changes.
            await UnitTest.AssertNoFurtherChanges(cdc, imp).ConfigureAwait(false);
        }

        [Test]
        public async Task CreateUpdateThenDelete()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<PostOrchestrator>();

            // Create rows, update and then delete - should result in nothing (quick create/delete within same batch are ignored)!
            var script =
                "INSERT INTO [Legacy].[Posts] ([PostsId], [Text], [Date]) VALUES (404, 'Blah 404', '2020-01-01T15:30:42')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Comments] ([CommentsId], [PostsId], [Text], [Date]) VALUES (4041, 404, 'Blah blah 4041', '2020-01-01T18:30:42')" + Environment.NewLine +
                "UPDATE [Legacy].[Comments] SET [Text] = 'Bananas' WHERE [CommentsId] = 4041" + Environment.NewLine +
                "DELETE FROM [Legacy].[Posts] WHERE [PostsId] = 404";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute picking up nothing.
            var imp = new InMemoryPublisher(logger);
            var cdc = new PostOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger);
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            ClassicAssert.NotNull(cdcr);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNotNull(cdcr.BatchTracker);
            ClassicAssert.IsTrue(cdcr.BatchTracker!.IsComplete);
            ClassicAssert.IsNotNull(cdcr.BatchTracker.CompletedDate);
            ClassicAssert.IsNotNull(cdcr.BatchTracker.CorrelationId);
            ClassicAssert.IsFalse(cdcr.BatchTracker.HasDataLoss);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(2, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.PublishCount);
            ClassicAssert.AreEqual(1, imp.GetEvents().Length);

            var events = imp.GetEvents();
            ClassicAssert.AreEqual(1, events.Length);

            UnitTest.AssertEvent("PostsTest-CreateUpdateThenDelete.txt", events[0]);

            // Ensure procesed correctly, execute again with no changes.
            await UnitTest.AssertNoFurtherChanges(cdc, imp).ConfigureAwait(false);
        }

        [Test]
        public async Task CreateUpdateWithUpcomingDelete()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<PostOrchestrator>();

            // Create rows, update and then delete - should result in nothing (quick create/delete within same batch are ignored)!
            var script =
                "INSERT INTO [Legacy].[Posts] ([PostsId], [Text], [Date]) VALUES (404, 'Blah 404', '2020-01-01T15:30:42')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Comments] ([CommentsId], [PostsId], [Text], [Date]) VALUES (4041, 404, 'Blah blah 4041', '2020-01-01T18:30:42')" + Environment.NewLine +
                "UPDATE [Legacy].[Comments] SET [Text] = 'Bananas' WHERE [CommentsId] = 4041" + Environment.NewLine +
                "DELETE FROM [Legacy].[Posts] WHERE [PostsId] = 404";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute picking up nothing.
            var imp = new InMemoryPublisher(logger);
            var cdc = new PostOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger) { MaxQuerySize = 1 };
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            ClassicAssert.NotNull(cdcr);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNotNull(cdcr.BatchTracker);
            ClassicAssert.IsTrue(cdcr.BatchTracker!.IsComplete);
            ClassicAssert.IsNotNull(cdcr.BatchTracker.CompletedDate);
            ClassicAssert.IsNotNull(cdcr.BatchTracker.CorrelationId);
            ClassicAssert.IsFalse(cdcr.BatchTracker.HasDataLoss);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(0, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(0, cdcr.ExecuteStatus?.PublishCount);
            ClassicAssert.AreEqual(0, imp.GetEvents().Length);

            // Now get the delete that we previously identified as an upcoming delete.
            imp = new InMemoryPublisher(logger);
            cdc = new PostOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger) { MaxQuerySize = 1 };
            cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            ClassicAssert.NotNull(cdcr);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNotNull(cdcr.BatchTracker);
            ClassicAssert.IsTrue(cdcr.BatchTracker!.IsComplete);
            ClassicAssert.IsNotNull(cdcr.BatchTracker.CompletedDate);
            ClassicAssert.IsNotNull(cdcr.BatchTracker.CorrelationId);
            ClassicAssert.IsFalse(cdcr.BatchTracker.HasDataLoss);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.PublishCount);
            ClassicAssert.AreEqual(1, imp.GetEvents().Length);

            var events = imp.GetEvents();
            ClassicAssert.AreEqual(1, events.Length);

            UnitTest.AssertEvent("PostsTest-CreateUpdateThenDelete.txt", events[0]);

            // Ensure procesed correctly, execute again with no changes.
            await UnitTest.AssertNoFurtherChanges(cdc, imp).ConfigureAwait(false);
        }

        [Test]
        public async Task UpdateSameVersion()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<PostOrchestrator>();

            var script =
                "UPDATE [Legacy].[Tags] SET [TEXT] = '#Other' WHERE [TagsId] = 3301" + Environment.NewLine +
                "UPDATE [Legacy].[Tags] SET [TEXT] = '#Tag' WHERE [TagsId] = 3301";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute picking up the changes resulting in a new event and corresponding version record.
            var imp = new InMemoryPublisher(logger);
            var cdc = new PostOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger);
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            ClassicAssert.NotNull(cdcr);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNotNull(cdcr.BatchTracker);
            ClassicAssert.IsTrue(cdcr.BatchTracker!.IsComplete);
            ClassicAssert.IsNotNull(cdcr.BatchTracker.CompletedDate);
            ClassicAssert.IsNotNull(cdcr.BatchTracker.CorrelationId);
            ClassicAssert.IsFalse(cdcr.BatchTracker.HasDataLoss);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(2, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.PublishCount);
            ClassicAssert.AreEqual(1, imp.GetEvents().Length);

            // Replay the database changes; i.e. change and then change back to original.
            script =
                "UPDATE [Legacy].[Tags] SET [TEXT] = '#Other' WHERE [TagsId] = 3301" + Environment.NewLine +
                "UPDATE [Legacy].[Tags] SET [TEXT] = '#Tag' WHERE [TagsId] = 3301";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute picking up the changes but should have no event due to same version.
            imp.Reset();
            cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            ClassicAssert.NotNull(cdcr);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNotNull(cdcr.BatchTracker);
            ClassicAssert.IsTrue(cdcr.BatchTracker!.IsComplete);
            ClassicAssert.IsNotNull(cdcr.BatchTracker.CompletedDate);
            ClassicAssert.IsNotNull(cdcr.BatchTracker.CorrelationId);
            ClassicAssert.IsFalse(cdcr.BatchTracker.HasDataLoss);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(2, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(0, cdcr.ExecuteStatus?.PublishCount);
            ClassicAssert.AreEqual(0, imp.GetEvents().Length);

            // Ensure procesed correctly, execute again with no changes.
            await UnitTest.AssertNoFurtherChanges(cdc, imp).ConfigureAwait(false);
        }

        [Test]
        public async Task DataLossStop()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<PostOrchestrator>();

            // Set up cdc tracking for data too far in the past; then update a Post to trigger CDC.
            var script = "UPDATE [Legacy].[Comments] SET [Text] = 'Bananas' WHERE [CommentsId] = 101";
            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);

            script = "UPDATE [NTangle].[PostsBatchTracking] SET PostsMinLsn = 0x00000000000000000001, PostsMaxLsn = 0x00000000000000000001";
            await sdb.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);

            await UnitTest.Delay().ConfigureAwait(false);

            // Execute picking up the changes _but_ discovering a data loss situation.
            var imp = new InMemoryPublisher(logger);
            var cdc = new PostOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger);
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            ClassicAssert.NotNull(cdcr);
            ClassicAssert.IsFalse(cdcr.IsSuccessful);
            //Assert.IsNull(cdcr.Batch);
            ClassicAssert.NotNull(cdcr.Exception);
            ClassicAssert.IsInstanceOf<DataConsistencyException>(cdcr.Exception);
            ClassicAssert.AreEqual("Unexpected data loss error for 'Legacy.Posts'; this indicates that the CDC data has probably been cleaned up before being successfully processed.", cdcr.Exception!.Message);
            ClassicAssert.IsNull(cdcr.ExecuteStatus);
        }

        [Test]
        public async Task DataLossContinue()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<PostOrchestrator>();

            // Set up cdc tracking for data too far in the past; then update a Post to trigger CDC.
            var script = "UPDATE [Legacy].[Comments] SET [Text] = 'Bananas' WHERE [CommentsId] = 101";
            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);

            script = "UPDATE [NTangle].[PostsBatchTracking] SET PostsMinLsn = 0x00000000000000000001, PostsMaxLsn = 0x00000000000000000001";
            await sdb.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);

            await UnitTest.Delay().ConfigureAwait(false);

            // Execute picking up the changes contuning with the data loss situation.
            var imp = new InMemoryPublisher(logger);
            var cdc = new PostOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger) { ContinueWithDataLoss = true };
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            ClassicAssert.NotNull(cdcr);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNotNull(cdcr.BatchTracker);
            ClassicAssert.IsTrue(cdcr.BatchTracker!.IsComplete);
            ClassicAssert.IsNotNull(cdcr.BatchTracker.CompletedDate);
            ClassicAssert.IsNotNull(cdcr.BatchTracker.CorrelationId);
            ClassicAssert.IsTrue(cdcr.BatchTracker.HasDataLoss);
            ClassicAssert.IsNull(cdcr.Exception);
        }

        [Test]
        public async Task RetryWhenIncomplete()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<PostOrchestrator>();

            var script = "UPDATE [Legacy].[Tags] SET [TEXT] = '#Other' WHERE [TagsId] = 3301";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute picking up the changes.
            var imp = new InMemoryPublisher(logger);
            var cdc = new PostOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger);
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            ClassicAssert.NotNull(cdcr);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNotNull(cdcr.BatchTracker);
            ClassicAssert.IsTrue(cdcr.BatchTracker!.IsComplete);
            ClassicAssert.IsNotNull(cdcr.BatchTracker.CompletedDate);
            ClassicAssert.IsNotNull(cdcr.BatchTracker.CorrelationId);
            ClassicAssert.IsFalse(cdcr.BatchTracker.HasDataLoss);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.PublishCount);
            ClassicAssert.AreEqual(1, imp.GetEvents().Length);

            // Set the last batch to incomplete and delete version tracking so it will publish event again - simulate batch failure.
            script =
                $"UPDATE [NTangle].[PostsBatchTracking] SET IsComplete = 0 WHERE CorrelationId = '{cdcr.BatchTracker.CorrelationId}'" + Environment.NewLine +
                "DELETE FROM [NTangle].[VersionTracking]";

            await sdb.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);

            // Execute picking up the existing batch and re-completing.
            imp.Reset();
            var cdcr2 = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr2, imp);

            // Assert/verify the results.
            ClassicAssert.NotNull(cdcr2);
            ClassicAssert.IsTrue(cdcr2.IsSuccessful);
            ClassicAssert.IsNotNull(cdcr2.BatchTracker);
            ClassicAssert.IsTrue(cdcr2.BatchTracker!.IsComplete);
            ClassicAssert.IsNotNull(cdcr2.BatchTracker.CompletedDate);
            ClassicAssert.IsNotNull(cdcr2.BatchTracker.CorrelationId);
            ClassicAssert.IsFalse(cdcr2.BatchTracker.HasDataLoss);
            ClassicAssert.IsNull(cdcr2.Exception);
            ClassicAssert.AreEqual(cdcr.BatchTracker.Id, cdcr2.BatchTracker.Id);
            ClassicAssert.AreEqual(cdcr.BatchTracker.CorrelationId, cdcr2.BatchTracker.CorrelationId);
            ClassicAssert.AreNotEqual(cdcr.ExecutionId, cdcr2.ExecutionId);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.PublishCount);
            ClassicAssert.AreEqual(1, imp.GetEvents().Length);
        }

        [Test]
        public async Task Explicit_SingleRoot()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<PostOrchestrator>();

            var imp = new InMemoryPublisher(logger);
            var cdc = new PostOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger);
            var cdcr = await cdc.ExecuteExplicitAsync([1]).ConfigureAwait(false);

            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            ClassicAssert.NotNull(cdcr);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNull(cdcr.BatchTracker);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.PublishCount);
            ClassicAssert.AreEqual(1, imp.GetEvents().Length);
        }

        [Test]
        public async Task Explicit_MultiChild()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<PostOrchestrator>();

            var imp = new InMemoryPublisher(logger);
            var cdc = new PostOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger);
            var cdcr = await cdc.ExecuteExplicitAsync(null, [102], [3301], [2002]).ConfigureAwait(false);

            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            ClassicAssert.NotNull(cdcr);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNull(cdcr.BatchTracker);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(3, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(3, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(3, cdcr.ExecuteStatus?.PublishCount);
            ClassicAssert.AreEqual(3, imp.GetEvents().Length);
        }

        [Test]
        public async Task Explicit_AssumeDeleted()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<PostOrchestrator>();

            var imp = new InMemoryPublisher(logger);
            var cdc = new PostOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger);
            var cdcr = await cdc.ExecuteExplicitAsync([404]).ConfigureAwait(false);

            UnitTest.WriteResult(cdcr, imp);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNull(cdcr.BatchTracker);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.PublishCount);
            ClassicAssert.AreEqual(1, imp.GetEvents().Length);

            // Now ignore where not found.
            imp.Reset();
            cdcr = await cdc.ExecuteExplicitAsync([404], options: new NTangle.Cdc.ExplicitOptions { AssumeDeleteWhereNotFound = false }).ConfigureAwait(false);

            UnitTest.WriteResult(cdcr, imp);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNull(cdcr.BatchTracker);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(0, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(0, cdcr.ExecuteStatus?.PublishCount);
            ClassicAssert.AreEqual(0, imp.GetEvents().Length);
        }

        [Test]
        public async Task Explicit_AssumeCreate()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<PostOrchestrator>();

            var imp = new InMemoryPublisher(logger);
            var cdc = new PostOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger);
            var cdcr = await cdc.ExecuteExplicitAsync([1]).ConfigureAwait(false);

            UnitTest.WriteResult(cdcr, imp);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNull(cdcr.BatchTracker);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.PublishCount);
            ClassicAssert.AreEqual(1, imp.GetEvents().Length);
            ClassicAssert.AreEqual("created", imp.GetEvents()[0].Action);

            // Should present as updated as versioned earlier.
            await db.SqlStatement("UPDATE [Legacy].[Posts] SET [Text] = 'Bananas' WHERE [PostsId] = 1").NonQueryAsync().ConfigureAwait(false);
            imp.Reset();
            cdcr = await cdc.ExecuteExplicitAsync([1]).ConfigureAwait(false);

            UnitTest.WriteResult(cdcr, imp);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNull(cdcr.BatchTracker);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.PublishCount);
            ClassicAssert.AreEqual(1, imp.GetEvents().Length);
            ClassicAssert.AreEqual("updated", imp.GetEvents()[0].Action);

            // Remove version and should present as updated as no assuming.
            await sdb.SqlStatement("DELETE FROM [NTangle].[VersionTracking]").NonQueryAsync().ConfigureAwait(false);
            imp.Reset();
            cdcr = await cdc.ExecuteExplicitAsync([1], options: new NTangle.Cdc.ExplicitOptions { AssumeCreateWhereNoVersion = false }).ConfigureAwait(false);

            UnitTest.WriteResult(cdcr, imp);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNull(cdcr.BatchTracker);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.PublishCount);
            ClassicAssert.AreEqual(1, imp.GetEvents().Length);
            ClassicAssert.AreEqual("updated", imp.GetEvents()[0].Action);
        }

        [Test]
        public async Task Explicit_AlwaysPublish()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<PostOrchestrator>();

            var imp = new InMemoryPublisher(logger);
            var cdc = new PostOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger);
            var cdcr = await cdc.ExecuteExplicitAsync([1]).ConfigureAwait(false);

            UnitTest.WriteResult(cdcr, imp);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNull(cdcr.BatchTracker);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.PublishCount);
            ClassicAssert.AreEqual(1, imp.GetEvents().Length);

            // No publish as checked.
            imp.Reset();
            cdcr = await cdc.ExecuteExplicitAsync([1]).ConfigureAwait(false);

            UnitTest.WriteResult(cdcr, imp);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNull(cdcr.BatchTracker);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(0, cdcr.ExecuteStatus?.PublishCount);
            ClassicAssert.AreEqual(0, imp.GetEvents().Length);

            // Always publish will now publish (ignores check).
            cdcr = await cdc.ExecuteExplicitAsync([1], options: new NTangle.Cdc.ExplicitOptions { AlwaysPublishEvents = true }).ConfigureAwait(false);

            UnitTest.WriteResult(cdcr, imp);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNull(cdcr.BatchTracker);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.PublishCount);
            ClassicAssert.AreEqual(1, imp.GetEvents().Length);
        }
    }
}