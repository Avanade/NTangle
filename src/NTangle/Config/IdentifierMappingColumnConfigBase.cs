// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using Newtonsoft.Json;
using OnRamp;
using OnRamp.Config;
using OnRamp.Database;
using System.Linq;

namespace NTangle.Config
{
    /// <summary>
    /// Represents the table join on condition configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [CodeGenClass("JoinOn", Title = "'JoinOn' object (database-driven)",
        Description = "The `JoinOn` object defines the join on characteristics for a `Join` object.",
        ExampleMarkdown = @"A YAML configuration example is as follows:
``` yaml

```")]
    [CodeGenCategory("Key", Title = "Provides the _key_ configuration.")]
    public abstract class IdentifierMappingColumnConfigBase<TParent> : ConfigBase<RootConfig, TParent> where TParent : ConfigBase, ITableReference
    {
        #region Key

        /// <summary>
        /// Gets or sets the name of the existing column that requires identifier mapping.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The name of of the existing column that requires identifier mapping.", IsMandatory = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the related table.
        /// </summary>
        [JsonProperty("schema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The schema name of the related table.",
            Description = "Defaults to the owning (parent) table schema.")]
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the related table.
        /// </summary>
        [JsonProperty("table", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The name of the related table.", IsMandatory = true)]
        public string? Table { get; set; }

        #endregion

        #region Non-Config

        /// <summary>
        /// Gets the corresponding (actual) database table configuration.
        /// </summary>
        public DbTable? DbTable { get; private set; }

        /// <summary>
        /// Gets the <see cref="Name"/> <see cref="DbColumn"/>.
        /// </summary>
        public DbColumn? DbColumn { get; private set; }

        #endregion

        /// <inheritdoc/>
        protected override void Prepare()
        {
            Schema = DefaultWhereNull(Schema, () => Parent!.Schema);

            DbColumn = Parent!.DbTable!.Columns.SingleOrDefault(x => x.Name == Name);
            if (DbColumn == null)
                throw new CodeGenException(this, null, $"Column '[{Name}]' does not exist for table '[{Parent!.Schema}].[{Parent!.Table}]'.");

            if (DbColumn.IsPrimaryKey)
                throw new CodeGenException(this, null, $"Column '[{Name}]' is part of the primary key for '[{Parent!.Schema}].[{Parent!.Table}]'; the primary key identifier mapping can only be configured using the parent `IdentifierMapping` feature.");

            DbTable = Root!.DbTables.FirstOrDefault(x => x.Schema == Schema && x.Name == Table);
            if (DbTable == null)
                throw new CodeGenException(this, nameof(Table), $"References table '[{Schema}].[{Table}]' that does not exist.");

            if (DbTable.Columns.Count(x => x.IsPrimaryKey) != 1)
                throw new CodeGenException(this, nameof(Table), $"References table '[{Schema}].[{Table}]' which must only have a single column representing the primary key.");
        }
    }
}