using CoreEx.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NTangle.Cdc;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Con = System.Console;

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

            if (result.Batch == null)
                Con.Out.WriteLine("Batch = null");
            else
            {
                Con.Out.WriteLine(string.Empty);
                Con.Out.WriteLine("Batch:");
                Con.Out.WriteLine($" Id={result.Batch.Id}");
                Con.Out.WriteLine($" IsComplete = {result.Batch.IsComplete}");
                Con.Out.WriteLine($" CreatedDate = {result.Batch.CreatedDate}");
                Con.Out.WriteLine($" CompletedDate = {result.Batch.CompletedDate?.ToString() ?? "null"}");
                Con.Out.WriteLine($" HasDataLoss = {result.Batch.HasDataLoss}");
                Con.Out.WriteLine($" CorrelationId = {result.Batch.CorrelationId}");
            }

            Con.Out.WriteLine(string.Empty);
            Con.Out.WriteLine($"Events: {(tep == null ? "null" : tep.Events.Count.ToString())}");
            if (tep != null && tep.Events.Count > 0)
            {
                foreach (var @event in tep.Events)
                {
                    var jt = JToken.Parse(Encoding.UTF8.GetString(@event));
                    Con.Out.WriteLine(jt.ToString(Formatting.Indented));
                    Con.Out.WriteLine(string.Empty);
                }
            }

            Con.Out.WriteLine(string.Empty);
        }

        /// <summary>
        /// Assert the event by comparing the JSON content against a text file.
        /// </summary>
        /// <param name="expected">The name of the file in the <c>Expected</c> folder.</param>
        /// <param name="binaryData">The event <see cref="BinaryData"/>.</param>
        /// <param name="exclude">The properties to exclude from the comparison.</param>
        public static void AssertEvent(string expected, BinaryData binaryData, params string[] exclude)
        {
            new CoreEx.Text.Json.JsonSerializer().Deserialize
            JsonSerializer.Default.TryApplyFilter(binaryData.ToString(), )
            JsonPropertyFilter(jt, null, new string[] { "id", "time", "correlationid", "data.etag" }.Concat(exclude));
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