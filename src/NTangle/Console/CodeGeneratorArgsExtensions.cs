// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using OnRamp;
using System;
using System.Collections.Generic;

namespace NTangle.Console
{
    /// <summary>
    /// Provides extenstion methods to <see cref="CodeGeneratorArgs"/>.
    /// </summary>
    public static class CodeGeneratorArgsExtensions
    {
        /// <summary>
        /// Gets the 'AppName' <see cref="CodeGeneratorArgsBase.Parameters"/> key name.
        /// </summary>
        public const string AppNameParamName = "AppName";

        /// <summary>
        /// Gets the <see cref="CodeGeneratorArgsBase.Parameters"/> value with a key of <see cref="AppNameParamName"/>.
        /// </summary>
        /// <param name="args">The <see cref="ICodeGeneratorArgs"/>.</param>
        /// <param name="throwWhereNotFound">Indicates to throw a <see cref="KeyNotFoundException"/> when the specified key is not found.</param>
        public static string GetAppName(this ICodeGeneratorArgs args, bool throwWhereNotFound = false) => (args ?? throw new ArgumentNullException(nameof(args))).GetParameter(AppNameParamName, throwWhereNotFound)!;

        /// <summary>
        /// Adds the <paramref name="appName"/> value with a key of <see cref="AppNameParamName"/> to the <see cref="CodeGeneratorArgsBase.Parameters"/>.
        /// </summary>
        /// <param name="args">The <see cref="ICodeGeneratorArgs"/>.</param>
        /// <param name="appName">The application name.</param>
        public static void SetAppName(this ICodeGeneratorArgs args, string? appName) => (args ?? throw new ArgumentNullException(nameof(args))).AddParameter(AppNameParamName, appName);
    }
}