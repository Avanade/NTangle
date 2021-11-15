using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTangle.Cdc;
using NTangle.Utility;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTangle.Test
{
    /// <summary>
    /// Provides test helping/utility functions.
    /// </summary>
    public static class UnitTest
    {
        /// <summary>
        /// Invokes the <see cref="Task.Delay(int)"/> for a standardized 5000 milliseconds.
        /// </summary>
        public static async Task Delay() => await Task.Delay(5000);

        /// <summary>
        /// Gets a console <see cref="ILogger"/>.
        /// </summary>
        /// <typeparam name="T">The logger <see cref="Type"/>.</typeparam>
        /// <returns>The <see cref="ILogger"/>.</returns>
        public static ILogger<T> GetLogger<T>() => LoggerFactory.Create(b =>
        {
            b.SetMinimumLevel(LogLevel.Trace);
            b.ClearProviders();
            b.AddConsole();
        }).CreateLogger<T>();

        /// <summary>
        /// Writes the <see cref="EntityOrchestratorResult"/> and <see cref="TestEventPublisher"/> output to the console.
        /// </summary>
        /// <param name="result">The <see cref="EntityOrchestratorResult"/>.</param>
        /// <param name="tep">The <see cref="TestEventPublisher"/>.</param>
        public static void WriteResult(EntityOrchestratorResult result, TestEventPublisher tep)
        {
            System.Console.Out.WriteLine(string.Empty);
            System.Console.Out.WriteLine("=========================");
            System.Console.Out.WriteLine("EntityOrchestratorResult");
            System.Console.Out.WriteLine($"ExecutionId = {result.ExecutionId}");
            System.Console.Out.WriteLine($"Success = {result.IsSuccessful}");
            System.Console.Out.WriteLine($"InitialCount = {result.ExecuteStatus?.InitialCount?.ToString() ?? "null"}");
            System.Console.Out.WriteLine($"ConsolidatedCount = {result.ExecuteStatus?.ConsolidatedCount?.ToString() ?? "null"}");
            System.Console.Out.WriteLine($"PublishCount = {result.ExecuteStatus?.PublishCount?.ToString() ?? "null"}");

            if (result.Exception != null)
                System.Console.Out.WriteLine($"Exception: {result.Exception.Message}");

            if (result.Batch == null)
                System.Console.Out.WriteLine("Batch = null");
            else
            {
                System.Console.Out.WriteLine(string.Empty);
                System.Console.Out.WriteLine("Batch:");
                System.Console.Out.WriteLine($" Id={result.Batch.Id}");
                System.Console.Out.WriteLine($" IsComplete = {result.Batch.IsComplete}");
                System.Console.Out.WriteLine($" CreatedDate = {result.Batch.CreatedDate}");
                System.Console.Out.WriteLine($" CompletedDate = {result.Batch.CompletedDate?.ToString() ?? "null"}");
                System.Console.Out.WriteLine($" HasDataLoss = {result.Batch.HasDataLoss}");
                System.Console.Out.WriteLine($" CorrelationId = {result.Batch.CorrelationId}");
            }

            System.Console.Out.WriteLine(string.Empty);
            System.Console.Out.WriteLine($"Events: {(tep == null ? "null" : tep.Events.Count.ToString())}");
            if (tep != null && tep.Events.Count > 0)
            {
                foreach (var @event in tep.Events)
                {
                    var jt = JToken.Parse(Encoding.UTF8.GetString(@event));
                    System.Console.Out.WriteLine(jt.ToString(Formatting.Indented));
                    System.Console.Out.WriteLine(string.Empty);
                }
            }
        }

        /// <summary>
        /// Assert the event by comparing the JSON content against a text file.
        /// </summary>
        /// <param name="expected">The name of the file in the <c>Expected</c> folder.</param>
        /// <param name="binaryData">The event <see cref="BinaryData"/>.</param>
        /// <param name="exclude">The properties to exclude from the comparison.</param>
        public static void AssertEvent(string expected, BinaryData binaryData, params string[] exclude)
        {
            var jt = JToken.Parse(binaryData.ToString());
            JsonPropertyFilter.JsonApply(jt, null, new string[] { "id", "time", "correlationid", "data.etag" }.Concat(exclude));
            var txt = jt.ToString(Formatting.Indented);

            var exp = File.ReadAllText(Path.Combine("Expected", expected));
            Assert.AreEqual(exp, txt);
        }

        /// <summary>
        /// Gets the event data deserialized to the requested type.
        /// </summary>
        /// <typeparam name="T">The data <see cref="Type"/>.</typeparam>
        /// <param name="binaryData">The event <see cref="BinaryData"/>.</param>
        /// <returns>The data value.</returns>
        public static T GetEventData<T>(BinaryData binaryData)
        {
            var jt = JToken.Parse(binaryData.ToString());
            var jd = jt["data"];
            return jd.ToObject<T>();
        }

        /// <summary>
        /// Execute the <see cref="IEntityOrchestrator"/> and ensure no further changes were found.
        /// </summary>
        /// <param name="eo">The <see cref="IEntityOrchestrator"/>.</param>
        /// <param name="tep">The <see cref="TestEventPublisher"/>.</param>
        /// <returns>The <see cref="EntityOrchestratorResult"/>.</returns>
        public static async Task<EntityOrchestratorResult> AssertNoFurtherChanges(IEntityOrchestrator eo, TestEventPublisher tep)
        {
            tep.Events.Clear();
            var cdcr = await eo.ExecuteAsync(null).ConfigureAwait(false);
            WriteResult(cdcr, tep);

            Assert.NotNull(cdcr);
            Assert.IsTrue(cdcr.IsSuccessful);
            Assert.IsNull(cdcr.Batch);
            Assert.IsNull(cdcr.Exception);
            Assert.AreEqual(0, tep.Events.Count);

            return cdcr;
        }
    }
}