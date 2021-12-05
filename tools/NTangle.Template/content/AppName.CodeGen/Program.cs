using NTangle.Console;
using System.Threading.Tasks;

namespace AppName.CodeGen
{
    /// <summary>
    /// <see href="https://github.com/Avanade/NTangle">NTangle</see>-based code-generation console program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Runs the code generation using the passed <paramref name="args"/> string.
        /// </summary>
        /// <param name="args">>The command-line arguments.</param>
        /// <returns><b>Zero</b> indicates success; otherwise, unsuccessful.</returns>
        /// <remarks>The specified connection string is for default development purposes only, this should be overriden using a command line argument ('<c>-cs|--connection-string</c>' or '<c>-cv|--connection-varname</c>') 
        /// or environment variable ('<c>SqlServerDemo_ConnectionString</c>'). Note: any '<c>.</c>' characters in the environment variable name must be replaced with an '<c>_</c>'.</remarks>
        public static async Task Main(string[] args) 
            => await new CodeGenConsole("Data Source=.;Initial Catalog=AppName;Integrated Security=True")
                .UseDbProvider(SqlServerDeployment.DbProject)
                .RunAsync(args).ConfigureAwait(false);
    }
}