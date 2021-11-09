// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using OnRamp;
using System.Threading.Tasks;

namespace NTangle.Console
{
    /// <summary>
    /// Provides the console capabilities.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The main entry point.
        /// </summary>
        /// <param name="args">The console arguments.</param>
        /// <returns><b>Zero</b> indicates success; otherwise, unsuccessful.</returns>
        public static async Task<int> Main(string[] args) => await new CodeGenConsole(CodeGeneratorArgs.Create<CodeGenConsole>()).RunAsync(args).ConfigureAwait(false);
    }
}