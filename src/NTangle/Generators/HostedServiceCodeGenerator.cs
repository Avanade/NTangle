// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using NTangle.Config;
using OnRamp.Generators;
using System.Collections.Generic;

namespace NTangle.Generators
{
    /// <summary>
    /// Represents a <see cref="TableConfig.Service"/> with a value of '<c>HostedService</c>' code generator.
    /// </summary>
    public class HostedServiceCodeGenerator : CodeGeneratorBase<RootConfig, TableConfig>
    {
        /// <inheritdoc/>
        protected override IEnumerable<TableConfig> SelectGenConfig(RootConfig config) => config.HostedServices;
    }
}