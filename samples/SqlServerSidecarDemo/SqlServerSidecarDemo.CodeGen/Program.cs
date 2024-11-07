using NTangle.CodeGen;

namespace SqlServerSidecarDemo.CodeGen;

internal class Program
{
    // Main invokes the CodeGenConsole passing in a default connection string to the database where CDC is to be enabled (this can be overridden using a command line argument or environment variable).
    public static Task<int> Main(string[] args) 
        => new CodeGenConsole("Data Source=.;Initial Catalog=SqlServerSidecarDemo;Integrated Security=True;TrustServerCertificate=true")
            .UseSidecarDatabase()
            .RunAsync(args);
}