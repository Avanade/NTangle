using NTangle.CodeGen;
using System.Threading.Tasks;

namespace ContactSync.OldApp.CodeGen;

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
    /// <remarks>The specified connection string is for default development purposes only; this can be overriden using a command line argument ('<c>-cs|--connection-string</c>' or '<c>-cv|--connection-varname</c>').</remarks>
    public static async Task Main(string[] args) => await new CodeGenConsole("Data Source=.;Initial Catalog=ContactSync.OldApp;Integrated Security=True;TrustServerCertificate=true").RunAsync(args).ConfigureAwait(false);
}