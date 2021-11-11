﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using Microsoft.Data.SqlClient;
using OnRamp;
using System;
using System.Data.Common;
using System.IO;
using System.Reflection;

namespace NTangle.Console
{
    /// <summary>
    /// <b>NTangle</b>-specific code-generation console that inherits from <see cref="OnRamp.Console.CodeGenConsoleBase"/>.
    /// </summary>
    /// <remarks>The <b>NTangle</b> capabilities are designed to be database provider agnostic, as such the underlying database access is managed via the common data capabilities such as <see cref="DbConnection"/>. Where no database provider
    /// is specified then SQL Server (via <see cref="UseSqlServer(Func{string, SqlConnection}?)"/>) is used as the default.</remarks>
    public class CodeGenConsole : OnRamp.Console.CodeGenConsoleBase
    {
        /// <summary>
        /// Gets the default configuration file name (see <see cref="ICodeGeneratorArgs.ConfigFileName"/>).
        /// </summary>
        public const string DefaultConfigFileName = "ntangle.yaml";

        /// <summary>
        /// Gets the default script file name (see <see cref="ICodeGeneratorArgs.ScriptFileName"/>) where <see cref="UseSqlServer(Func{string, SqlConnection}?)"/> is used.
        /// </summary>
        public const string DefaultSqlServerScriptFileName = "SqlServerDacpac.yaml";

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
            Args.ConfigFileName ??= DefaultConfigFileName;
            UseSqlServer();
        }

        /// <summary>
        /// Uses (overrides) the execution script file or embedded resource name.
        /// </summary>
        /// <param name="scriptFileName">The execution script file or embedded resource name.</param>
        /// <returns>The current instance to support fluent-style method-chaining.</returns>
        public CodeGenConsole UseScript(string scriptFileName)
        {
            Args.ScriptFileName = scriptFileName ?? throw new ArgumentNullException(scriptFileName);
            return this;
        }

        /// <summary>
        /// Uses (overrides) the configuration file name (defaults to <see cref="DefaultConfigFileName"/>).
        /// </summary>
        /// <param name="configFileName">The configuration file name.</param>
        /// <returns>The current instance to support fluent-style method-chaining.</returns>
        public CodeGenConsole UseConfig(string configFileName)
        {
            Args.ConfigFileName = configFileName ?? throw new ArgumentNullException(configFileName);
            return this;
        }

        /// <summary>
        /// Uses (overrides) the database connection creation to leverage the SQL Server <see cref="SqlConnection"/> and sets the <see cref="UseScript(string)">script</see> to <see cref="DefaultSqlServerScriptFileName"/>.
        /// </summary>
        /// <param name="sqlConnectionCreator">The optional <paramref name="sqlConnectionCreator"/> to enable advanced <see cref="SqlConnection"/> creation; defaults to using <see cref="SqlConnection(string)"/> where not specified.</param>
        /// <returns>The current instance to support fluent-style method-chaining.</returns>
        /// <remarks>Also invokes <see cref="CodeGeneratorArgsExtensions.SetDbProvider(ICodeGeneratorArgs, string?)"/> passing '<c>SqlServer</c>'.</remarks>
        public CodeGenConsole UseSqlServer(Func<string, SqlConnection>? sqlConnectionCreator = null)
        {
            UseScript(DefaultSqlServerScriptFileName);
            Args.DbConnectionCreator = sqlConnectionCreator ?? (cs => new SqlConnection(cs));
            Args.SetDbProvider("SqlServer");
            return this;
        }
    }
}