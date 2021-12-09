// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using NTangle.Config;
using OnRamp.Console;
using OnRamp.Utility;
using System;
using System.IO;
using Con = System.Console;

namespace NTangle.ArtefactGenerate.Tool
{
    /// <summary>
    /// Tool to generate the JSON Schema and Markdown documentation artefacts.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        internal static void Main()
        {
            var di = new DirectoryInfo(CodeGenConsole.GetBaseExeDirectory());
            var root = di.Parent.Parent;
            if (root.Name != "NTangle")
                throw new InvalidOperationException($"The inferred root path should be 'NTangle'; inferred '{root.FullName}' incorrectly.");

            // First up, generate the JSON Schema.
            var fn = Path.Combine(root.FullName, "schemas", "ntangle.json");
            Con.WriteLine("JSON Schema Generation");
            Con.WriteLine($"  Generating: {fn}");

            JsonSchemaGenerator.Generate<RootConfig>(fn, "NTangle - https://github.com/Avanade/ntangle");

            Con.WriteLine(" Complete.");
            Con.WriteLine("");

            // Now, generate the corresponding markdown documentation files.
            var dn = Path.Combine(root.FullName, "docs", "generated");
            Con.WriteLine("Markdown Documentation Generation");
            Con.WriteLine($" Directory: {dn}");

            MarkdownDocumentationGenerator.Generate<RootConfig>((type, csa) => $"{csa.Name.ToLowerInvariant()}.md", directory: dn, includeExample: false, addBreaksBetweenSections: true, fileCreation: fn => Con.WriteLine($"  Generating: {fn}"));

            Con.WriteLine(" Complete.");
            Con.WriteLine("");
        }
    }
}