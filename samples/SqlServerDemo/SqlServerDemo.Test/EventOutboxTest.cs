using CoreEx.Entities;
using CoreEx.Events;
using CoreEx.Json;
using NTangle.Test;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SqlServerDemo.Publisher.Data;
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
                "DELETE FROM [Outbox].[EventOutbox]" + Environment.NewLine +
                "DELETE FROM [Outbox].[EventOutboxData]";

            await SqlServerUnitTest.GetDatabase().SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
        }

        [Test]
        public async Task OutboxDequeuePublisher()
        {
            using var db = SqlServerUnitTest.GetDatabase();
            var logger = UnitTest.GetLogger<ContactOrchestrator>();

            // Update contact 1.
            var script = "UPDATE [Legacy].[Contact] SET [Phone] = '000' WHERE [ContactId] = 1";
            await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
            await UnitTest.Delay().ConfigureAwait(false);

            // Execute should pick up and allocate all new global identifiers.
            var eoe = new EventOutboxEnqueue(db, UnitTest.GetLogger<EventOutboxEnqueue>());
            var ep = new EventPublisher(null, new CoreEx.Text.Json.CloudEventSerializer(), eoe);
            var cdc = new ContactOrchestrator(db, ep, JsonSerializer.Default, UnitTest.GetSettings(), logger, new IdentifierGenerator());
            var cdcr = await cdc.ExecuteAsync().ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, null);

            // Assert/verify the results.
            ClassicAssert.NotNull(cdcr);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNotNull(cdcr.BatchTracker);
            ClassicAssert.IsTrue(cdcr.BatchTracker.IsComplete);
            ClassicAssert.IsNotNull(cdcr.BatchTracker.CompletedDate);
            ClassicAssert.IsNotNull(cdcr.BatchTracker.CorrelationId);
            ClassicAssert.IsFalse(cdcr.BatchTracker.HasDataLoss);
            ClassicAssert.IsNull(cdcr.Exception);

            // Now execute OutboxDequeuePublisher to get the event using different partition key.
            var ims = new InMemorySender();
            var eod = new EventOutboxDequeue(db, ims, UnitTest.GetLogger<EventOutboxDequeue>());
            await eod.DequeueAndSendAsync(10, "Bananas", null, CancellationToken.None).ConfigureAwait(false);

            ClassicAssert.AreEqual(0, ims.GetEvents().Length);

            // Now execute OutboxDequeuePublisher to get the event using correct partition key.
            await eod.DequeueAndSendAsync(10, "Contact", null, CancellationToken.None).ConfigureAwait(false);

            var events = ims.GetEvents();
            ClassicAssert.AreEqual(1, events.Length);
            UnitTest.AssertEvent("EventOutboxTest-OutboxDequeuePublisher.txt", events[0], "data.globalId", "data.globalAlternateContactId", "data.address.globalId", "data.address.globalAlternateAddressId");

            // Make sure there are no events left to dequeue.
            ClassicAssert.AreEqual(0, await db.SqlStatement("SELECT COUNT(*) FROM [Outbox].[EventOutbox] WHERE [DequeuedDate] IS NULL").ScalarAsync<int>().ConfigureAwait(false));
        }
    }
}