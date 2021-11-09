using NTangle.Console;
using System.Threading.Tasks;

namespace SqlServerDemo.CodeGen
{
    internal class Program
    {
        public static async Task Main(string[] args) => await CodeGenConsole.Create("Data Source=.;Initial Catalog=SqlServerDemo;Integrated Security=True").RunAsync(args).ConfigureAwait(false);
    }
}