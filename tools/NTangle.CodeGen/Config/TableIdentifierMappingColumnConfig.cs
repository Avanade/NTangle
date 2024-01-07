// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using OnRamp.Config;

namespace NTangle.CodeGen.Config
{
    /// <summary>
    /// Represents the table join on condition configuration.
    /// </summary>
    [CodeGenClass("TableMapping", Title = "'TableMapping' object (database-driven)",
        Description = "The `TableMapping` object defines one or more related table identifier mappings.",
        ExampleMarkdown = @"A YAML configuration example is as follows:
``` yaml

```")]
    public class TableIdentifierMappingColumnConfig : IdentifierMappingColumnConfigBase<TableConfig> 
    {
        /// <inheritdoc/>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("Mappings", Name);
    }
}