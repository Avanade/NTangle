// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using NTangle.Config;
using OnRamp.Generators;
using System.Collections.Generic;

namespace NTangle.Generators
{
    /// <summary>
    /// Represents a <see cref="RootConfig.CdcEnable"/>-driven code generator.
    /// </summary>
    public class CdcEnableCodeGenerator : CodeGeneratorBase<RootConfig>
    {
        /// <inheritdoc/>
        protected override IEnumerable<RootConfig> SelectGenConfig(RootConfig config) => config.CdcEnabledTables.Count > 0 ? config.SelectGenResult : null!;
    }
}