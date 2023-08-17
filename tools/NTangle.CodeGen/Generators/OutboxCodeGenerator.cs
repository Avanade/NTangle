// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using NTangle.CodeGen.Config;
using OnRamp.Generators;
using System.Collections.Generic;

namespace NTangle.CodeGen.Generators
{
    /// <summary>
    /// Represents a <see cref="RootConfig.Outbox"/>-driven code generator.
    /// </summary>
    public class OutboxCodeGenerator : CodeGeneratorBase<RootConfig>
    {
        /// <inheritdoc/>
        protected override IEnumerable<RootConfig> SelectGenConfig(RootConfig config) => IsTrue(config.Outbox) ? config.SelectGenResult : null!;
    }
}