using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTangle.Cdc;
using NTangle.Data;
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
        public static IDatabase GetDatabase()
        {
            var cb = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
            var cs = cb.GetConnectionString("SqlDb");
            return new Database(() => new SqlConnection(cs));
        }

        public static async Task Delay() => await Task.Delay(5000);

        public static ILogger<T> GetLogger<T>() => LoggerFactory.Create(b =>
        {
            b.SetMinimumLevel(LogLevel.Trace);
            b.ClearProviders();
            b.AddConsole();
        }).CreateLogger<T>();

        public static void WriteResult(EntityOrchestratorResult result, TestEventPublisher tep)
        {
            TestContext.WriteLine(string.Empty);
            TestContext.WriteLine("=========================");
            TestContext.WriteLine("EntityOrchestratorResult");
            TestContext.WriteLine($"Success: {result.IsSuccessful}");
            if (result.Exception != null)
                TestContext.WriteLine($"Exception: {result.Exception.Message}");

            if (result.Batch == null)
                TestContext.WriteLine($"Batch - null");
            else
                TestContext.WriteLine($"Batch - Id={result.Batch.Id}, IsComplete={result.Batch.IsComplete}, CreatedDate={result.Batch.CreatedDate}, CompletedDate={result.Batch.CompletedDate}, HasDataLoss={result.Batch.HasDataLoss}, CorrelationId={result.Batch.CorrelationId}, ExecutionId={result.ExecutionId}");

            TestContext.WriteLine(string.Empty);
            TestContext.WriteLine($"Events: {(tep == null ? "null" : tep.Events.Count.ToString())}");
            if (tep != null && tep.Events.Count > 0)
            {
                foreach (var @event in tep.Events)
                {
                    var jt = JToken.Parse(Encoding.UTF8.GetString(@event));
                    TestContext.WriteLine(jt.ToString(Formatting.Indented));
                    TestContext.WriteLine(string.Empty);
                }
            }
        }

        public static void AssertEvent(string expected, byte[] bytes, params string[] exclude)
        {
            var jt = JToken.Parse(Encoding.UTF8.GetString(bytes));
            JsonPropertyFilter.JsonApply(jt, null, new string[] { "id", "time", "correlationid", "data.etag" }.Concat(exclude));
            var txt = jt.ToString(Formatting.Indented);

            var exp = File.ReadAllText(Path.Combine("Expected", expected));
            Assert.AreEqual(exp, txt);
        }

        public static async Task AssertNoFurtherChanges(IEntityOrchestrator eo, TestEventPublisher tep)
        {
            tep.Events.Clear();
            var cdcr = await eo.ExecuteAsync(null).ConfigureAwait(false);
            UnitTest.WriteResult(cdcr, tep);

            Assert.NotNull(cdcr);
            Assert.IsTrue(cdcr.IsSuccessful);
            Assert.IsNull(cdcr.Batch);
            Assert.IsNull(cdcr.Exception);
            Assert.AreEqual(0, tep.Events.Count);
        }
    }
}