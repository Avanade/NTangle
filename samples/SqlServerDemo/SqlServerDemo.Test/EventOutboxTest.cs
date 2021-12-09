using NTangle;
using NTangle.Test;
using NUnit.Framework;
using SqlServerDemo.Publisher.Data;
using SqlServerDemo.Publisher.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SqlServerDemo.Test
{
    [TestFixture]
    [NonParallelizable]
    public class EventOutboxTest
    {
        [SetUp]
        public async Task InitTest()
        {
            await new ContactTest().InitTest().ConfigureAwait(false);

            // Reset the event outbox tables..
            var script =
                "DELETE FROM [NTangle].[EventOutbox]" + Environment.NewLine +
                "DELETE FROM [NTangle].[EventOutboxData]";

            await SqlServerUnitTest.GetDatabase().SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
        }

        [Test]
        public async Task OutboxDequeuePublisher()
        {
            using var db = SqlServerUnitTest.GetDatabase();
            var logger = UnitTest.GetLogger<ContactCdcOrchestrator>();

            // Update contact 1.
            var script = "UPDATE [Legacy].[Contact] SET [Phone] = '000' WHERE [ContactId] = 1";
            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute should pick up and allocate all new global identifiers.
            var oep = new OutboxEventPublisher(db);
            var cdc = new ContactCdcOrchestrator(db, oep, logger, new IdentifierGenerator());
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, null);

            // Assert/verify the results.
            Assert.NotNull(cdcr);
            Assert.IsTrue(cdcr.IsSuccessful);
            Assert.IsNotNull(cdcr.Batch);
            Assert.IsTrue(cdcr.Batch.IsComplete);
            Assert.IsNotNull(cdcr.Batch.CompletedDate);
            Assert.IsNotNull(cdcr.Batch.CorrelationId);
            Assert.IsFalse(cdcr.Batch.HasDataLoss);
            Assert.IsNull(cdcr.Exception);

            // Now execute OutboxDequeuePublisher to get the event using different partition key.
            var tep = new TestEventPublisher();
            var odp = new OutboxDequeuePublisher(db, tep, UnitTest.GetLogger<OutboxDequeuePublisher>());
            await odp.DequeueAndPublishAsync(10, "Bananas", CancellationToken.None).ConfigureAwait(false);

            Assert.AreEqual(0, tep.Events.Count);

            // Now execute OutboxDequeuePublisher to get the event using correct partition key.
            await odp.DequeueAndPublishAsync(10, "Contact", CancellationToken.None).ConfigureAwait(false);

            Assert.AreEqual(1, tep.Events.Count);
            UnitTest.AssertEvent("ContactTest-GenerateAllIdentifiers.txt", tep.Events[0], "data.globalId", "data.globalAlternateContactId", "data.address.globalId", "data.address.globalAlternateAddressId");

            // Make sure there are no events left to dequeue.
            Assert.AreEqual(0, await db.SqlStatement("SELECT COUNT(*) FROM [NTangle].[EventOutbox] WHERE [DequeuedDate] IS NULL").ScalarAsync<int>().ConfigureAwait(false));
        }
    }
}