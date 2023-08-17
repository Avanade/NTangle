// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using NTangle.CodeGen.Config;
using OnRamp.Generators;
using System.Collections.Generic;

namespace NTangle.CodeGen.Generators
{
    /// <summary>
    /// Represents a <see cref="TableConfig"/>-driven code generator.
    /// </summary>
    public class TableCodeGenerator : CodeGeneratorBase<RootConfig, TableConfig>
    {
        /// <inheritdoc/>
        protected override IEnumerable<TableConfig> SelectGenConfig(RootConfig config) => config.Tables!;
    }
}