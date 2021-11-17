using NTangle.Console;
using System.Threading.Tasks;

namespace AppName.CodeGen
{
    /// <summary>
    /// The console program.
    /// </summary>
    internal class Program
    {
        // Main invokes the CodeGenConsole passing in a default connection string to the database where CDC is to be enabled (this can be overridden using a command line argument or environment variable).
        public static async Task Main(string[] args) => await CodeGenConsole.Create("Data Source=.;Initial Catalog=AppName;Integrated Security=True").UseDbProvider().RunAsync(args).ConfigureAwait(false);
    }
}