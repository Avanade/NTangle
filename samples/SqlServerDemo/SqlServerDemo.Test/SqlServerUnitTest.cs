using DbUp;
using DbUp.Engine;
using DbUp.Engine.Output;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NTangle.Data;
using NTangle.Test;
using NUnit.Framework;
using OnRamp.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SqlServerDemo.Test
{
    /// <summary>
    /// Provides test helping/utility functions.
    /// </summary>
    [SetUpFixture]
    public static class SqlServerUnitTest
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
        public static void SetUpDatabase()
        {
            var cs = UnitTest.GetConfig("SqlServerDemo_").GetConnectionString("SqlDb");
            var ls = new LoggerSink(UnitTest.GetLogger<LoggerSink>());
            var di = GetDatabaseDirectory();
            var scripts = new List<SqlScript>();
            var grp = 0;

            Console.WriteLine("DATABASE SETUP - Start...");

            // Drop the database where pre-existing.
            DropDatabase.For.SqlDatabase(cs, ls);

            // Create a new empty database.
            EnsureDatabase.For.SqlDatabase(cs, ls);

            // Add the pre scripts(s).
            AddScript(scripts, grp++, di, "Pre.Deploy.sql");

            // Add all the scripts within each folder.
            var sdi = new DirectoryInfo(Path.Combine(di.FullName, "Schema", "NTangle"));
            if (!sdi.Exists)
                throw new InvalidOperationException($"Directory does not exist: {sdi.FullName}");

            AddScripts(scripts, grp++, sdi, "Generated");
            AddScripts(scripts, grp++, sdi, "Tables");
            AddScripts(scripts, grp++, sdi, "Types");
            AddScripts(scripts, grp++, sdi, "Stored Procedures");

            // Add the post scripts(s).
            AddScripts(scripts, grp++, di, "Generated");

            // Apply all the scripts.
            var result = DeployChanges.To.SqlDatabase(cs)
                .WithScripts(scripts)
                .WithoutTransaction()
                .LogTo(ls)
                .Build()
                .PerformUpgrade();

            if (!result.Successful)
                throw new InvalidOperationException($"Database deployment failed: {result.Error.Message}");

            Console.WriteLine("DATABASE SETUP - Finished...");
            Console.WriteLine(string.Empty);
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