using CoreEx.Events;
using CoreEx.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NTangle.Cdc;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using Stj = System.Text.Json;
using System.Threading.Tasks;
using Con = System.Console;
using CoreEx.Configuration;
using NUnit.Framework.Legacy;

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
        /// Gets (builds) the <see cref="IConfigurationRoot"/>.
        /// </summary>
        /// <returns>The <see cref="IConfigurationRoot"/>.</returns>
        public static IConfigurationRoot GetConfig(string prefix) => new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).AddJsonFile("appsettings.json").AddEnvironmentVariables(prefix).Build();

        /// <summary>
        /// Gets (builds) the <see cref="SettingsBase"/>.
        /// </summary>
        /// <returns>The <see cref="SettingsBase"/>.</returns>
        public static SettingsBase GetSettings(string prefix = null) => new DefaultSettings(GetConfig(prefix ?? "ntangle_unittest"));

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
        /// Writes the <see cref="EntityOrchestratorResult"/> and <see cref="InMemoryPublisher"/> output to the console.
        /// </summary>
        /// <param name="result">The <see cref="EntityOrchestratorResult"/>.</param>
        /// <param name="imp">The <see cref="InMemoryPublisher"/>.</param>
        public static void WriteResult(EntityOrchestratorResult result, InMemoryPublisher imp)
        {
            Con.Out.WriteLine(string.Empty);
            Con.Out.WriteLine("=========================");
            Con.Out.WriteLine("EntityOrchestratorResult");
            Con.Out.WriteLine($"ExecutionId = {result.ExecutionId}");
            Con.Out.WriteLine($"Success = {result.IsSuccessful}");
            Con.Out.WriteLine($"InitialCount = {result.ExecuteStatus?.InitialCount?.ToString() ?? "null"}");
            Con.Out.WriteLine($"ConsolidatedCount = {result.ExecuteStatus?.ConsolidatedCount?.ToString() ?? "null"}");
            Con.Out.WriteLine($"PublishCount = {result.ExecuteStatus?.PublishCount?.ToString() ?? "null"}");

            if (result.Exception != null)
                Con.Out.WriteLine($"Exception: {result.Exception.Message}");

            if (result.BatchTracker == null)
                Con.Out.WriteLine("Batch = null");
            else
            {
                Con.Out.WriteLine(string.Empty);
                Con.Out.WriteLine("Batch:");
                Con.Out.WriteLine($" Id={result.BatchTracker.Id}");
                Con.Out.WriteLine($" IsComplete = {result.BatchTracker.IsComplete}");
                Con.Out.WriteLine($" CreatedDate = {result.BatchTracker.CreatedDate}");
                Con.Out.WriteLine($" CompletedDate = {result.BatchTracker.CompletedDate?.ToString() ?? "null"}");
                Con.Out.WriteLine($" HasDataLoss = {result.BatchTracker.HasDataLoss}");
                Con.Out.WriteLine($" CorrelationId = {result.BatchTracker.CorrelationId}");
            }

            if (imp == null)
                Con.Out.WriteLine("Events: null");
            else
            {
                var events = imp.GetEvents();
                Con.Out.WriteLine(string.Empty);
                Con.Out.WriteLine($"Events: {events.Length}");
                if (imp != null && events.Length > 0)
                {
                    foreach (var @event in events)
                    {
                        var json = JsonSerializer.Default.Serialize(@event, JsonWriteFormat.Indented);
                        Con.Out.WriteLine(json);
                        Con.Out.WriteLine(string.Empty);
                    }
                }
            }

            Con.Out.WriteLine(string.Empty);
        }

        /// <summary>
        /// Assert the event by comparing the JSON content against a text file.
        /// </summary>
        /// <param name="expected">The name of the file in the <c>Expected</c> folder.</param>
        /// <param name="actual">The <see cref="EventData"/>.</param>
        /// <param name="exclude">The properties to exclude from the comparison.</param>
        public static void AssertEvent(string expected, EventData actual, params string[] exclude)
        {
            JsonSerializer.Default.TryApplyFilter(actual, new string[] { "id", "timestamp", "correlationid", "etag", "key", "value.etag" }.Concat(exclude), out string json, JsonPropertyFilter.Exclude);
            var je = (Stj.JsonElement)JsonSerializer.Default.Deserialize(json);
            var exp = File.ReadAllText(Path.Combine("Expected", expected));
            ClassicAssert.AreEqual(exp, JsonSerializer.Default.Serialize(je, JsonWriteFormat.Indented));
        }

        /// <summary>
        /// Assert the event by comparing the JSON content against a text file.
        /// </summary>
        /// <param name="expected">The name of the file in the <c>Expected</c> folder.</param>
        /// <param name="actual">The <see cref="EventData"/>.</param>
        /// <param name="exclude">The properties to exclude from the comparison.</param>
        public static void AssertEvent(string expected, EventSendData actual, params string[] exclude)
        {
            var jn = Stj.Nodes.JsonNode.Parse(actual.Data);
            CoreEx.Text.Json.JsonFilterer.Apply(jn, new string[] { "id", "time", "correlationid", "etag", "key", "data.etag" }.Concat(exclude), JsonPropertyFilter.Exclude);
            var exp = File.ReadAllText(Path.Combine("Expected", expected));
            ClassicAssert.AreEqual(exp, jn.ToJsonString(new Stj.JsonSerializerOptions { WriteIndented = true }));
        }

        /// <summary>
        /// Gets the event data deserialized to the requested type.
        /// </summary>
        /// <typeparam name="T">The data <see cref="Type"/>.</typeparam>
        /// <param name="eventData">The event <see cref="BinaryData"/>.</param>
        /// <returns>The data value.</returns>
        public static T GetEventData<T>(EventData eventData) => (T)eventData.Value;

        /// <summary>
        /// Execute the <see cref="IEntityOrchestrator"/> and ensure no further changes were found.
        /// </summary>
        /// <param name="eo">The <see cref="IEntityOrchestrator"/>.</param>
        /// <param name="imp">The <see cref="InMemoryPublisher"/>.</param>
        /// <returns>The <see cref="EntityOrchestratorResult"/>.</returns>
        public static async Task<EntityOrchestratorResult> AssertNoFurtherChanges(IEntityOrchestrator eo, InMemoryPublisher imp)
        {
            imp.Reset();
            var cdcr = await eo.ExecuteAsync().ConfigureAwait(false);
            WriteResult(cdcr, imp);

            ClassicAssert.NotNull(cdcr);
            ClassicAssert.IsTrue(cdcr.IsSuccessful);
            ClassicAssert.IsNull(cdcr.BatchTracker);
            ClassicAssert.IsNull(cdcr.Exception);
            ClassicAssert.AreEqual(0, imp.GetEvents().Length);

            return cdcr;
        }
    }
}