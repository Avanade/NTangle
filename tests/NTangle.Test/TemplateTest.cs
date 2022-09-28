using CoreEx.Database.SqlServer;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NTangle.Test
{
    [TestFixture]
    public class TemplateTest
    {
        private static bool _firstTime = true;
        private static DirectoryInfo _rootDir;
        private static DirectoryInfo _nugetDir;
        private static DirectoryInfo _unitTestsDir;

        /// <summary>
        /// Executes the specified command.
        /// </summary>
        private static (int exitCode, string stdOut) ExecuteCommand(string filename, string arguments = null, string workingDirectory = null, int? milliseconds = null)
        {
            TestContext.WriteLine("**********************************************************************");
            TestContext.WriteLine($"dir> {workingDirectory ?? _rootDir.FullName}");
            TestContext.WriteLine($"cmd> {filename} {arguments}");
            TestContext.WriteLine("");

            var sbo = new StringBuilder();
            var sbe = new StringBuilder();

            using var process = new Process();
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WorkingDirectory = workingDirectory ?? _rootDir.FullName;
            process.StartInfo.FileName = filename;
            process.StartInfo.Arguments = arguments;
            process.OutputDataReceived += new DataReceivedEventHandler((sender, e) => sbo.AppendLine(e.Data));
            process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => sbe.AppendLine(e.Data));

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit(milliseconds == null ? 30000 : milliseconds.Value);
            if (!process.HasExited)
                process.Kill();

            process.WaitForExit();
            var exitCode = process.ExitCode;
            process.Close();

            TestContext.WriteLine(sbo.ToString());
            TestContext.Error.WriteLine(sbe.ToString());
            TestContext.WriteLine($"res> {exitCode}");
            TestContext.WriteLine("**********************************************************************");

            return (exitCode, sbo.ToString());
        }

        /// <summary>
        /// Perform one-time set up activities; remove any existing, create and publish nugets locally, install dotnet template.
        /// </summary>
        public static void OneTimeSetUp()
        {
            if (!_firstTime)
                return;

            _firstTime = false;

            // Determine directories.
            _rootDir = new DirectoryInfo(TestContext.CurrentContext.WorkDirectory);
            while (_rootDir.Name != "NTangle")
            {
                _rootDir = _rootDir.Parent;
            }

            _unitTestsDir = new DirectoryInfo(Path.Combine(_rootDir.FullName, ".unittests"));

            // Remove previous tests and create fresh.
            if (_unitTestsDir.Exists)
                _unitTestsDir.Delete(true);

            _unitTestsDir.Create();

            // Remove existing cached nuget packages.
            var (exitCode, stdOut) = ExecuteCommand("dotnet", "nuget locals global-packages --list");
            Assert.GreaterOrEqual(0, exitCode);

            var packages = new DirectoryInfo(stdOut.Replace("info : global-packages: ", string.Empty).Replace("global-packages: ", string.Empty).Replace(Environment.NewLine, ""));
            Assert.IsTrue(packages.Exists);
            foreach (var di in packages.EnumerateDirectories().Where(x => x.Name.StartsWith("ntangle")))
            {
                di.Delete(true);
            }

            _nugetDir = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "nuget-publish"));
            foreach (var fi in _nugetDir.EnumerateFiles().Where(x => x.Name.StartsWith("NTangle.")))
            {
                fi.Delete();
            }

            // Build and package (nuget) - only local package, no deployment.
            Assert.GreaterOrEqual(0, ExecuteCommand("dotnet", "clean ./src/ntangle", _rootDir.FullName).exitCode, "dotnet clean");
            Assert.GreaterOrEqual(0, ExecuteCommand("dotnet", "build ./src/ntangle --force", _rootDir.FullName).exitCode, "dotnet build");
            Assert.GreaterOrEqual(0, ExecuteCommand("dotnet", "pack ./src/ntangle", _rootDir.FullName).exitCode, "dotnet pack");

            foreach (var fi in new DirectoryInfo(Path.Combine(_rootDir.FullName, "src", "ntangle")).EnumerateFiles("*.nupkg", SearchOption.AllDirectories))
            {
                Assert.GreaterOrEqual(0, ExecuteCommand("dotnet", $"nuget push {fi.FullName} --source {_nugetDir.FullName}", _rootDir.FullName).exitCode, $"dotnet nuget push {fi.FullName}");
            }

            // Uninstall the template solution from local package.
            ExecuteCommand("dotnet", $"new -u ntangle.template --nuget-source {_nugetDir.FullName}", _rootDir.FullName);

            // Install the template solution from local package.
            Assert.GreaterOrEqual(0, ExecuteCommand("dotnet", $"new -i ntangle.template --nuget-source {_nugetDir.FullName}", _rootDir.FullName).exitCode, "install ntangle.template");
        }

        [Test]
        public async Task DbProject_DbEx()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Assert.Inconclusive("This test is *only* supported on the Windows platform; otherwise, the Process.Start results in the following error: A fatal error was encountered. The library 'libhostpolicy.so' required to execute the application was not found in '/home/runner/.dotnet'.");

            OneTimeSetUp();

            var appName = "TestDbEx";

            // Make directory and create solution from/using template. 
            var dir = Path.Combine(_unitTestsDir.FullName, appName);
            Directory.CreateDirectory(dir);
            Assert.Zero(ExecuteCommand("dotnet", $"new ntangle --dbproject dbex", dir).exitCode, "dotnet new ntangle");

            // Restore nuget packages from our repository.
            Assert.Zero(ExecuteCommand("dotnet", $"restore -s {_nugetDir.FullName}", dir).exitCode, "dotnet restore");

            // Get the database script.
            var cfn = Path.Combine(_rootDir.FullName, "tools", "NTangle.Template", "create-database.sql");
            if (!File.Exists(cfn))
                Assert.Fail($"Unable to find database create script: {cfn}");

            // Database: Drop and Create database.
            Assert.Zero(ExecuteCommand("dotnet", "run drop", Path.Combine(dir, $"{appName}.Database")).exitCode, "dotnet run drop [database]");
            Assert.Zero(ExecuteCommand("dotnet", "run create", Path.Combine(dir, $"{appName}.Database")).exitCode, "dotnet run create [database]");

            // Read the set-up database script and ignore all lines until "DECLARE" found; previous not required.
            var lines = File.ReadAllLines(cfn);
            bool found = false;
            int i = 0;
            for (; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("DECLARE"))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                Assert.Fail($"DECLARE statement not found in: {cfn}");

            var sql = string.Join(Environment.NewLine, lines[i..]);
            Assert.Zero(ExecuteCommand("dotnet", $"run execute \"{sql}\"", Path.Combine(dir, $"{appName}.Database")).exitCode, "dotnet run execute [execute create-database.sql]");

            // CodeGen: Execute code-generation.
            Assert.Zero(ExecuteCommand("dotnet", "run", Path.Combine(dir, $"{appName}.CodeGen")).exitCode, "dotnet run all [codegen]");

            // Database: Execute database deploy.
            Assert.Zero(ExecuteCommand("dotnet", "run deploy", Path.Combine(dir, $"{appName}.Database")).exitCode, "dotnet run deploy [database]");

            // Publisher : Execute publisher and check event sent.
            await ChangeSomeData(appName).ConfigureAwait(false);
            var (_, stdOut) = ExecuteCommand("dotnet", "run Interval=00:00:01 OutboxInterval=00:00:02", Path.Combine(dir, $"{appName}.Publisher"), 15000);
            Assert.IsTrue(stdOut.Contains("\"source\": \"/database/cdc/legacy/contact/1\""), "Expected published event content not found in stdout.");
        }

        private static async Task ChangeSomeData(string appName)
        {
            var cs = $"Data Source=.;Initial Catalog={appName};Integrated Security=True;TrustServerCertificate=true";
            var db = new SqlServerDatabase(() => new SqlConnection(cs));
            await db.SqlStatement("UPDATE [Legacy].[Contact] SET [Phone] = '000' WHERE [ContactId] = 1").NonQueryAsync().ConfigureAwait(false);
        }

        [Test]
        public void DbProject_Dacpac()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Assert.Inconclusive("This test is *only* supported on the Windows platform; otherwise, the Process.Start results in the following error: A fatal error was encountered. The library 'libhostpolicy.so' required to execute the application was not found in '/home/runner/.dotnet'.");

            OneTimeSetUp();

            var appName = "TestDacpac";

            // Make directory and create solution from/using template. 
            var dir = Path.Combine(_unitTestsDir.FullName, appName);
            Directory.CreateDirectory(dir);
            Assert.Zero(ExecuteCommand("dotnet", $"new ntangle --dbproject dacpac", dir).exitCode, "dotnet new ntangle");

            // Restore nuget packages from our repository.
            Assert.Zero(ExecuteCommand("dotnet", $"restore -s {_nugetDir.FullName}", dir).exitCode, "dotnet restore");

            // Get the database script.
            var cfn = Path.Combine(_rootDir.FullName, "tools", "NTangle.Template", "create-database.sql");
            if (!File.Exists(cfn))
                Assert.Fail($"Unable to find database create script: {cfn}");

            var nfn = Path.Combine(dir, "create-database.sql");
            File.WriteAllText(nfn, File.ReadAllText(cfn).Replace("FooBar", appName));

            Assert.Zero(ExecuteCommand("sqlcmd", $"/i\"{nfn}\"").exitCode, "sqlcmd create-database.sql");

            // CodeGen: Execute code-generation.
            Assert.Zero(ExecuteCommand("dotnet", "run", Path.Combine(dir, $"{appName}.CodeGen")).exitCode, "dotnet run all [codegen]");

            // Publisher : Build successfully.
            Assert.Zero(ExecuteCommand("dotnet", "build", Path.Combine(dir, $"{appName}.Publisher")).exitCode, "dotnet build [publisher]");

            // Inconclusive from here on in I am afraid :-(
            Assert.Inconclusive("Stopped after code-gen. The test does not build or publish the Database project. It also does not run the Publisher; only validates the build.");
        }
    }
}