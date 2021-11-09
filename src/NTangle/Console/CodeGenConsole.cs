// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using OnRamp;
using System;
using System.IO;
using System.Reflection;

namespace NTangle.Console
{
    /// <summary>
    /// <b>NTangle</b>-specific code-generation console that inherits from <see cref="OnRamp.Console.CodeGenConsoleBase"/>.
    /// </summary>
    /// <remarks>Command line parsing: https://natemcmaster.github.io/CommandLineUtils/ </remarks>
    public class CodeGenConsole : OnRamp.Console.CodeGenConsoleBase
    {
        private string _script = "SqlServerDacpac.yaml";
        private string _config = "ntangle.yaml";

        /// <summary>
        /// Gets the default masthead text.
        /// </summary>
        /// <remarks>Defaults to 'NTangle Code-Gen Tool' formatted using <see href="http://www.patorjk.com/software/taag/#p=display&amp;f=Calvin%20S&amp;t=NTangle%20Code-Gen%20Tool%0A"/>.</remarks>
        public const string DefaultMastheadText = @"
╔╗╔╔╦╗┌─┐┌┐┌┌─┐┬  ┌─┐  ╔═╗┌─┐┌┬┐┌─┐  ╔═╗┌─┐┌┐┌  ╔╦╗┌─┐┌─┐┬  
║║║ ║ ├─┤││││ ┬│  ├┤   ║  │ │ ││├┤───║ ╦├┤ │││   ║ │ ││ ││  
╝╚╝ ╩ ┴ ┴┘└┘└─┘┴─┘└─┘  ╚═╝└─┘─┴┘└─┘  ╚═╝└─┘┘└┘   ╩ └─┘└─┘┴─┘
";

        /// <summary>
        /// Creates a new instance of the <see cref="CodeGenConsole"/> class defaulting to <see cref="Assembly.GetCallingAssembly"/>.
        /// </summary>
        /// <param name="connectionString">The default connection string.</param>
        /// <param name="appName">The application name; defaults to the <see cref="OnRamp.Console.CodeGenConsoleBase.GetBaseExeDirectory"/> <see cref="DirectoryInfo.Parent"/> directory name.</param>
        /// <param name="outputDirectory">The output path/directory; defaults to the resulting <see cref="OnRamp.Console.CodeGenConsoleBase.GetBaseExeDirectory"/> <see cref="DirectoryInfo.Parent"/>.</param>
        /// <returns>The <see cref="CodeGenConsole"/> instance.</returns>
        public static CodeGenConsole Create(string connectionString, string? appName = null, string? outputDirectory = null) => Create(new Assembly[] { Assembly.GetCallingAssembly() }, connectionString, appName, outputDirectory);

        /// <summary>
        /// Creates a new instance of the <see cref="CodeGenConsole"/> class.
        /// </summary>
        /// <param name="assemblies">The list of additional assemblies to probe for resources.</param>
        /// <param name="connectionString">The default connection string.</param>
        /// <param name="appName">The application name; defaults to the <see cref="OnRamp.Console.CodeGenConsoleBase.GetBaseExeDirectory"/> <see cref="DirectoryInfo.Parent"/> directory name.</param>
        /// <param name="outputDirectory">The output path/directory; defaults to the resulting <see cref="OnRamp.Console.CodeGenConsoleBase.GetBaseExeDirectory"/> <see cref="DirectoryInfo.Parent"/>.</param>
        /// <returns>The <see cref="CodeGenConsole"/> instance.</returns>
        public static CodeGenConsole Create(Assembly[] assemblies, string connectionString, string? appName = null, string? outputDirectory = null)
        {
            var args = new CodeGeneratorArgs { OutputDirectory = string.IsNullOrEmpty(outputDirectory) ? new DirectoryInfo(GetBaseExeDirectory()).Parent : new DirectoryInfo(outputDirectory) };
            args.AddAssembly(typeof(CodeGenConsole).Assembly);
            args.AddAssembly(assemblies);
            args.ConnectionString = connectionString;
            args.SetAppName(appName ?? new DirectoryInfo(GetBaseExeDirectory()).Parent.Name);
            return new CodeGenConsole(args);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenConsole"/> class.
        /// </summary>
        internal CodeGenConsole(CodeGeneratorArgs args) : base(typeof(CodeGenConsole).Assembly, args, Assembly.GetEntryAssembly()!.GetName().Name, options: OnRamp.Console.SupportedOptions.All)
        {
            MastheadText = DefaultMastheadText;
            Args.CreateConnectionStringEnvironmentVariableName ??= csargs => $"{csargs.GetAppName()?.Replace(".", "_", StringComparison.InvariantCulture)}_ConnectionString";
            Args.ScriptFileName ??= _script;
            Args.ConfigFileName ??= _config;
        }

        /// <summary>
        /// Sets (overrides) the execution script file or embedded resource name (defaults to <c>DatabaseCdc.yaml</c>).
        /// </summary>
        /// <param name="script">The execution script file or embedded resource name.</param>
        /// <returns>The current instance to support fluent-style method-chaining.</returns>
        public CodeGenConsole Script(string script)
        {
            _script = script ?? throw new ArgumentNullException(script);
            return this;
        }

        /// <summary>
        /// Sets (overrides) the configuration file name (defaults to <c>ntangle.yaml</c>).
        /// </summary>
        /// <param name="config">The configuration file name.</param>
        /// <returns>The current instance to support fluent-style method-chaining.</returns>
        public CodeGenConsole Config(string config)
        {
            _config = config ?? throw new ArgumentNullException(config);
            return this;
        }
    }
}