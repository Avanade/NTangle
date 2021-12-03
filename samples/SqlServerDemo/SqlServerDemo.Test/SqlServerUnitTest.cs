using DbEx;
using DbEx.Console;
using DbEx.Migration;
using DbUp.Engine;
using DbUp.Engine.Output;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NTangle.Data.SqlServer;
using NTangle.Test;
using NUnit.Framework;
using OnRamp.Console;
using SqlServerDemo.CodeGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SqlServerDemo.Test
{
    /// <summary>
    /// Provides test helping/utility functions.
    /// </summary>
    [SetUpFixture]
    public class SqlServerUnitTest
    {
        /// <summary>
        /// Gets the <see cref="SqlServerDatabase"/> used for testing.
        /// </summary>
        /// <returns>The <see cref="SqlServerDatabase"/>.</returns>
        public static IDatabase GetDatabase()
        {
            var cs = UnitTest.GetConfig("SqlServerDemo_").GetConnectionString("SqlDb");
            Console.WriteLine($"ConnectionString: {cs}");
            return new SqlServerDatabase(() => new SqlConnection(cs));
        }

        /// <summary>
        /// Set up the database, drop, create and apply scripts.
        /// </summary>
        [OneTimeSetUp]
        public static async Task SetUpDatabase()
        {
            var cs = UnitTest.GetConfig("SqlServerDemo_").GetConnectionString("SqlDb");

            await SqlServerMigratorConsole
                .Create<Program>(cs)
                .RunAsync(MigrationCommand.DropAndAll).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the base database project directory.
        /// </summary>
        private static DirectoryInfo GetDatabaseDirectory()
        {
            var di = new DirectoryInfo(CodeGenConsoleBase.GetBaseExeDirectory());

            while (true)
            {
                if (di.Name == "SqlServerDemo")
                    return di.GetDirectories().Where(x => x.Name == "SqlServerDemo.Database").Single();

                if (di.Parent == di.Root)
                    break;

                di = di.Parent;
            }

            throw new InvalidOperationException($"Unable to navigate to the SqlServerDemo.Database directory from: {CodeGenConsoleBase.GetBaseExeDirectory()}.");
        }

        /// <summary>
        /// Add the SQL script.
        /// </summary>
        private static void AddScript(List<SqlScript> scripts, int group, DirectoryInfo di, string filename)
        {
            var fi = new FileInfo(Path.Combine(di.FullName, filename));
            if (!fi.Exists)
                throw new InvalidOperationException($"File does not exist: {fi.FullName}");

            var script = SqlScript.FromFile(fi.FullName);
            script.SqlScriptOptions.RunGroupOrder = group;
            scripts.Add(script);
        }

        /// <summary>
        /// Adds all SQL scripts in directory and sub-directories.
        /// </summary>
        private static void AddScripts(List<SqlScript> scripts, int group, DirectoryInfo di, string folder)
        {
            var ndi = new DirectoryInfo(Path.Combine(di.FullName, folder));
            if (!ndi.Exists)
                return;

            foreach (var fi in ndi.GetFiles("*.sql", SearchOption.AllDirectories))
            {
                AddScript(scripts, group, fi.Directory, fi.Name);
            }
        }

        /// <summary>
        /// Represents a DbUp to ILogger sink.
        /// </summary>
        private class LoggerSink : IUpgradeLog
        {
            private readonly ILogger _logger;

            public LoggerSink(ILogger logger) => _logger = logger;

            public void WriteError(string format, params object[] args) => _logger.LogError(format, args);

            public void WriteInformation(string format, params object[] args) => _logger.LogInformation(format, args);

            public void WriteWarning(string format, params object[] args) => _logger.LogWarning(format, args);
        }
    }
}