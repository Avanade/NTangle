using CoreEx.Entities;
using CoreEx.Events;
using CoreEx.Json;
using NTangle.Test;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SqlServerSidecarDemo.Publisher.Data;
using SqlServerSidecarDemo.Publisher.Entities;

namespace SqlServerSidecarDemo.Test
{
    [TestFixture]
    [NonParallelizable]
    public class ContactTest
    {
        [SetUp]
        public async Task InitTest()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();

            // Create some data.
            var script =
                "DELETE FROM [Legacy].[Contact]" + Environment.NewLine +
                "DELETE FROM [Legacy].[Address]" + Environment.NewLine +
                "DELETE FROM [Legacy].[ContactMapping]" + Environment.NewLine +
                "INSERT INTO [Legacy].[Address] ([AddressId], [Street1], [AlternateAddressId]) VALUES (11, 'Side', 88)" + Environment.NewLine +
                "INSERT INTO [Legacy].[Address] ([AddressId], [Street1]) VALUES (88, 'Main')" + Environment.NewLine +
                "INSERT INTO [Legacy].[Contact] ([ContactId], [Name], [Phone], [Active], [AddressId], [AlternateContactId]) VALUES (1, 'Name1', '123', 1, 11, 2)" + Environment.NewLine +
                "INSERT INTO [Legacy].[Contact] ([ContactId], [Name], [Phone], [Active]) VALUES (2, 'Name2', '456', 1)" + Environment.NewLine +
                "INSERT INTO [Legacy].[Contact] ([ContactId], [Name], [Phone], [Active]) VALUES (3, 'Name3', '789', 1)" + Environment.NewLine +
                "INSERT INTO [Legacy].[ContactMapping] ([ContactMappingId], [ContactId], [UniqueId]) VALUES (1, 1, CONVERT(uniqueidentifier, '1afc7f5c-ffa0-4741-81cf-f12eAAb822bf'))";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Reset our view of CDC as we do not want to include in the next data capture.
            script =
                "DELETE FROM [NTangle].[IdentifierMapping]" + Environment.NewLine +
                "DELETE FROM [NTangle].[VersionTracking]" + Environment.NewLine +
                "DELETE FROM [NTangle].[ContactBatchTracking]";

            await sdb.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);

            await SqlServerSidecarUnitTest.ResetBatchAsync(db, sdb, "[NTangle].[spContactBatchReset]").ConfigureAwait(false);
        }

        [Test]
        public async Task GenerateAllIdentifiers()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<ContactOrchestrator>();

            // Update contact 1.
            var script = "UPDATE [Legacy].[Contact] SET [Phone] = '000' WHERE [ContactId] = 1";
            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute should pick up and allocate all new global identifiers.
            var imp = new InMemoryPublisher(logger);
            var cdc = new ContactOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger, new IdentifierGenerator());
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

            var events = imp.GetEvents();
            ClassicAssert.AreEqual(1, events.Length);

            UnitTest.AssertEvent("ContactTest-GenerateAllIdentifiers.txt", events[0], "value.globalId", "value.globalAlternateContactId", "value.address.globalId", "value.address.globalAlternateAddressId");

            // Check the event identifiers.
            var c = UnitTest.GetEventData<ContactCdc>(events[0]);
            ClassicAssert.NotNull(c.GlobalId);
            ClassicAssert.NotNull(c.GlobalAlternateContactId);
            ClassicAssert.NotNull(c.Address?.GlobalId);
            ClassicAssert.NotNull(c.Address?.GlobalAlternateAddressId);

            // Update contact 1 again.
            script = "UPDATE [Legacy].[Address] SET [Street1] = '1st' WHERE [AddressId] = 11" + Environment.NewLine;
            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute should pick up and reuse all the previous global identifiers.
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
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.PublishCount);
            events = imp.GetEvents();
            ClassicAssert.AreEqual(1, events.Length);

            // Check the event identifiers.
            var c2 = UnitTest.GetEventData<ContactCdc>(events[0]);
            ClassicAssert.AreEqual(c.GlobalId, c2.GlobalId);
            ClassicAssert.AreEqual(c.GlobalAlternateContactId, c2.GlobalAlternateContactId);
            ClassicAssert.AreEqual(c.Address?.GlobalId, c2.Address?.GlobalId);
            ClassicAssert.AreEqual(c.Address?.GlobalAlternateAddressId, c2.Address?.GlobalAlternateAddressId);
        }

        [Test]
        public async Task NoPublishSameHash()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<ContactOrchestrator>();

            // Update contact 1.
            var script = "UPDATE [Legacy].[Contact] SET [Phone] = '8888' WHERE [ContactId] = 1";
            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            var imp = new InMemoryPublisher(logger);
            var cdc = new ContactOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger, new IdentifierGenerator());
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            ClassicAssert.NotNull(cdcr);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);

            var events = imp.GetEvents();
            ClassicAssert.AreEqual(1, events.Length);

            // Update contact 1 again with same value.
            await db.SqlStatement("UPDATE [Legacy].[Contact] SET [Phone] = '7777' WHERE [ContactId] = 1").NonQueryAsync().ConfigureAwait(false);
            await db.SqlStatement("UPDATE [Legacy].[Contact] SET [Phone] = '8888' WHERE [ContactId] = 1").NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            imp.Reset();
            cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            ClassicAssert.NotNull(cdcr);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);

            events = imp.GetEvents();
            ClassicAssert.AreEqual(0, events.Length);
        }

        [Test]
        public async Task UsePreassignedIdentifiers()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<ContactOrchestrator>();

            // Update contact 1 and pre-assign some global identifiers.
            var script =
                "UPDATE [Legacy].[Address] SET [Street1] = '1st' WHERE [AddressId] = 11";

            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);

            script =
                "INSERT INTO [NTangle].[IdentifierMapping] ([Schema], [Table], [Key], [GlobalId]) VALUES ('Legacy', 'Contact', '1', 'C1')" + Environment.NewLine +
                "INSERT INTO [NTangle].[IdentifierMapping] ([Schema], [Table], [Key], [GlobalId]) VALUES ('Legacy', 'Contact', '2', 'C2')" + Environment.NewLine +
                "INSERT INTO [NTangle].[IdentifierMapping] ([Schema], [Table], [Key], [GlobalId]) VALUES ('Legacy', 'Address', '88', 'C88')";

            await sdb.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute should pick up and reuse all the previous global identifiers.
            var imp = new InMemoryPublisher(logger);
            var cdc = new ContactOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger, new IdentifierGenerator());
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

            UnitTest.AssertEvent("ContactTest-UsePreassignedIdentifiers.txt", events[0], "value.globalId", "value.globalAlternateContactId", "value.address.globalId", "value.address.globalAlternateAddressId");

            // Check the event identifiers.
            var c = UnitTest.GetEventData<ContactCdc>(events[0]);
            ClassicAssert.AreEqual("C1", c.GlobalId);
            ClassicAssert.NotNull(c.GlobalAlternateContactId);
            ClassicAssert.NotNull(c.Address?.GlobalId);
            ClassicAssert.AreEqual("C88", c.Address?.GlobalAlternateAddressId);
        }

        [Test]
        public async Task Explicit_Root()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<ContactOrchestrator>();

            var imp = new InMemoryPublisher(logger);
            var cdc = new ContactOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger, new IdentifierGenerator());
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
        }

        [Test]
        public async Task Explicit_Child()
        {
            using var db = SqlServerSidecarUnitTest.GetDatabase();
            using var sdb = SqlServerSidecarUnitTest.GetSidecarDatabase();
            var logger = UnitTest.GetLogger<ContactOrchestrator>();

            var imp = new InMemoryPublisher(logger);
            var cdc = new ContactOrchestrator(db, sdb, imp, JsonSerializer.Default, UnitTest.GetSettings(), logger, new IdentifierGenerator());
            var cdcr = await cdc.ExecuteExplicitAsync(null, [11]).ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, imp);

            // Assert/verify the results.
            ClassicAssert.NotNull(cdcr);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNull(cdcr.BatchTracker);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.InitialCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.ConsolidatedCount);
            ClassicAssert.AreEqual(1, cdcr.ExecuteStatus?.PublishCount);
        }
    }
}