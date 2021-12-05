using DbEx.Console;
using System.Threading.Tasks;

namespace SqlServerDemo.Database
{
    /// <summary>
    /// <see href="https://github.com/Avanade/DbEx">DbEx</see>-based database deployment migration console program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Runs the database deployment migration using the passed <paramref name="args"/> string.
        /// </summary>
        /// <param name="args">>The command-line arguments.</param>
        /// <returns><b>Zero</b> indicates success; otherwise, unsuccessful.</returns>
        /// <remarks>The specified connection string is for default development purposes only, this should be overriden using a command line argument ('<c>-cs|--connection-string</c>' or '<c>-cv|--connection-varname</c>') 
        /// or environment variable ('<c>SqlServerDemo_ConnectionString</c>'). Note: any '<c>.</c>' characters in the environment variable name must be replaced with an '<c>_</c>'.</remarks>
        public static async Task<int> Main(string[] args) => 
            await new SqlServerMigratorConsole("Data Source=.;Initial Catalog=SqlServerDemo;Integrated Security=True")
                .RunAsync(args).ConfigureAwait(false);
    }
}