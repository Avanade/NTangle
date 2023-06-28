// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx;
using Newtonsoft.Json;
using OnRamp;
using OnRamp.Config;
using DbEx.DbSchema;
using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NTangle.Config
{
    /// <summary>
    /// Represents the table join configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [CodeGenClass("Join", Title = "'Join' object (database-driven)",
        Description = "The `Join` object defines a join to another (or same) table within the logical CDC entity. "
            + " The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude selected columns; with the `AliasColumns` providing a means to rename where required.",
        ExampleMarkdown = @"A YAML configuration example is as follows:
``` yaml
```")]
    [CodeGenCategory("Key", Title = "Provides the _key_ configuration.")]
    [CodeGenCategory("JoinTo", Title = "Provides the _join to_ configuration.")]
    [CodeGenCategory("Database", Title = "Provides the _database_ configuration.")]
    [CodeGenCategory("Columns", Title = "Provides the _Columns_ configuration.")]
    [CodeGenCategory(".NET", Title = "Provides the _.NET_ configuration.")]
    [CodeGenCategory("IdentifierMapping", Title = "Provides the _identifier mapping_ configuration.")]
    [CodeGenCategory("Collections", Title = "Provides related child (hierarchical) configuration.")]
    public class JoinConfig : ConfigBase<RootConfig, TableConfig>, ITableReference
    {
        /// <inheritdoc/>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("Join", Name);

        #region Key

        /// <summary>
        /// Gets or sets the unqiue name.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The unique name.", IsMandatory = true, IsImportant = true,
            Description = "A unique name is required where the same `Table` is referenced more than once within a logical CDC entity. However, generally, this will represent the unique name of the table within the database.")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the table to join.
        /// </summary>
        [JsonProperty("schema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The schema name of the table to join.",
            Description = "Defaults to `Cdc.Schema`; i.e. same schema.")]
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the name of the table to join.
        /// </summary>
        [JsonProperty("table", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The name of the table to join.",
            Description = "Defaults to `Name`. This is used to specify the actual underlying database table name (required where the `Name` has been changed to enable uniqueness).")]
        public string? Table { get; set; }

        /// <summary>
        /// Gets or sets the `Schema` and `Table` alias name.
        /// </summary>
        [JsonProperty("alias", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The `Schema` and `Table` alias name.",
            Description = "Will automatically default where not specified.")]
        public string? Alias { get; set; }

        /// <summary>
        /// Gets or sets the join type option.
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The SQL join type.", IsImportant = true, Options = new string[] { "Cdc", "Inner", "Left", "Right", "Full" },
            Description = "Defaults to `Cdc`. The `Cdc` value indicates this is a related secondary table (within a logical CDC entity) that also has Change Data Capture turned on and equally needs to be monitored for changes.")]
        public string? Type { get; set; }

        #endregion

        #region JoinTo

        /// <summary>
        /// Gets or sets the name of the parent table to join to.
        /// </summary>
        [JsonProperty("joinTo", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("JoinTo", Title = "The name of the table to join to (must be previously specified).", IsImportant = true,
            Description = "Defaults to parent `Table.Name`.")]
        public string? JoinTo { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the parent table to join to.
        /// </summary>
        [JsonProperty("joinToSchema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("JoinTo", Title = "The schema name of the table to join to.", IsImportant = true,
            Description = "Defaults to parent `Table.Schema`.")]
        public string? JoinToSchema { get; set; }

        /// <summary>
        /// Get or sets the join cardinality.
        /// </summary>
        [JsonProperty("joinCardinality", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("JoinTo", Title = "The join cardinality being whether there is a One-to-Many or One-to-One relationship.", Options = new string[] { "OneToMany", "OneToOne" },
            Description = "Defaults to `OneToMany`. This represents the Parent (`JoinTo`) to child (_this_) relationship.")]
        public string? JoinCardinality { get; set; }

        #endregion

        #region Database

        /// <summary>
        /// Indicates whether to enable `Cdc` within the database for the tables that participate.
        /// </summary>
        [JsonProperty("cdcEnable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Database", Title = "Indicates whether to enable `Cdc` within the database for the tables that participate.",
            Description = "Defaults to `false`. This option can be overridden for each underlying table referenced.")]
        public bool? CdcEnable { get; set; }

        /// <summary>
        /// Gets or sets the query size multiplier for the CDC-Join.
        /// </summary>
        [JsonProperty("querySizeMultiplier", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Database", Title = "The query size multiplier for the CDC-Join.",
            Description = "Defaults to `1.0`. This is applied to the execute stored procedure `@MaxQuerySize` parameter to allow tailoring on the join query (`TOP`) sizes to optimize selection. Must be greater than zero and less than or equal to 100.")]
        public decimal? QuerySizeMultiplier { get; set; }

        #endregion

        #region Columns

        /// <summary>
        /// Gets or sets the list of `Column` names to be included in the underlying generated output.
        /// </summary>
        [JsonProperty("includeColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Columns", Title = "The list of `Column` names to be included in the underlying generated output.", IsImportant = true,
            Description = "Where not specified this indicates that all `Columns` are to be included.")]
        public List<string>? IncludeColumns { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names to be excluded from the underlying generated output.
        /// </summary>
        [JsonProperty("excludeColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Columns", Title = "The list of `Column` names to be excluded from the underlying generated output.", IsImportant = true,
            Description = "Where not specified this indicates no `Columns` are to be excluded.")]
        public List<string>? ExcludeColumns { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` and `Alias` pairs to enable column renaming.
        /// </summary>
        [JsonProperty("aliasColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Columns", Title = "The list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column renaming.", IsImportant = true,
            Description = "Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`")]
        public List<string>? AliasColumns { get; set; }

        #endregion

        #region .NET

        /// <summary>
        /// Gets or sets the .NET model name.
        /// </summary>
        [JsonProperty("model", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty(".NET", Title = "The .NET model name.",
            Description = "Defaults to `Name`.")]
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the .NET property name.
        /// </summary>
        [JsonProperty("property", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty(".NET", Title = "The .NET property name.",
            Description = "Defaults to `Model` where `JoinCardinality` is `OneToOne`; otherwise, it will be the `Model` pluralized.")]
        public string? Property { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names that should be included (in addition to the primary key) for a logical delete.
        /// </summary>
        [JsonProperty("includeColumnsOnDelete", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection(".NET", Title = "The list of `Column` names that should be included (in addition to the primary key) for a logical delete.",
           Description = "Where a column is not specified in this list its corresponding .NET property will be automatically cleared by the `CdcDataOrchestrator` as the data is technically considered as non-existing.")]
        public List<string>? IncludeColumnsOnDelete { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names that should be excluded from the generated ETag (used for the likes of duplicate send tracking).
        /// </summary>
        [JsonProperty("excludeColumnsFromETag", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection(".NET", Title = "The list of `Column` names that should be excluded from the generated ETag (used for the likes of duplicate send tracking).",
            Description = "Defaults to `CodeGeneration.CdcExcludeColumnsFromETag`.")]
        public List<string>? ExcludeColumnsFromETag { get; set; }

        #endregion

        #region IdentifierMapping

        /// <summary>
        /// Indicates whether to perform Identifier Mapping (mapping to `GlobalId`) for the primary key.
        /// </summary>
        [JsonProperty("identifierMapping", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("IdentifierMapping", Title = "Indicates whether to perform Identifier Mapping (mapping to `GlobalId`) for the primary key.", IsImportant = true,
           Description = "This indicates whether to create a new `GlobalId` property on the _entity_ to house the global mapping identifier to be the reference outside of the specific database realm as a replacement to the existing primary key column(s).")]
        public bool? IdentifierMapping { get; set; }

        #endregion

        #region Collections

        /// <summary>
        /// Gets or sets the corresponding <see cref="JoinOnConfig"/> collection.
        /// </summary>
        [JsonProperty("on", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `JoinOn` collection.")]
        public List<JoinOnConfig>? On { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="JoinIdentifierMappingColumnConfig"/> collection.
        /// </summary>
        [JsonProperty("mappings", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `JoinMapping` collection.")]
        public List<JoinIdentifierMappingColumnConfig>? Mappings { get; set; }

        #endregion

        #region Non-Config

        /// <summary>
        /// Gets or sets the <see cref="On"/> select columns; i.e. those without a specific statement.
        /// </summary>
        public List<JoinOnConfig> OnSelectColumns => On!.Where(x => x.ToStatement == null).ToList();

        /// <summary>
        /// Gets the selected column configurations.
        /// </summary>
        public List<JoinColumnConfig> Columns { get; } = new List<JoinColumnConfig>();

        /// <summary>
        /// Gets the corresponding (actual) database table configuration.
        /// </summary>
        public DbTableSchema? DbTable { get; private set; }

        /// <summary>
        /// Gets the <see cref="JoinTo"/> alias.
        /// </summary>
        public string? JoinToAlias { get; private set; }

        /// <summary>
        /// Gets the list of primary key columns.
        /// </summary>
        public List<JoinColumnConfig> PrimaryKeyColumns { get; } = new List<JoinColumnConfig>();

        /// <summary>
        /// Gets the join (linked) hierarchy (this and its parent up).
        /// </summary>
        public List<JoinConfig> JoinHierarchy { get; private set; } = new List<JoinConfig>();

        /// <summary>
        /// Gets the join (linked) hierarchy (this and its parent up) in reverse order.
        /// </summary>
        public List<JoinConfig> JoinHierarchyReverse => JoinHierarchy.Reverse<JoinConfig>().ToList();

        /// <summary>
        /// Gets the list of joined "directly related" children.
        /// </summary>
        public List<JoinConfig> JoinCdcChildren => Parent!.Joins!.Where(x => x.JoinTo == Name && x.JoinToSchema == Schema && CompareNullOrValue(x.Type, "Cdc")).ToList();

        /// <summary>
        /// Gets the list of non-CDC joined "directly related" children.
        /// </summary>
        public List<JoinConfig> JoinNonCdcChildren => Parent!.Joins!.Where(x => x.JoinTo == Name && x.JoinToSchema == Schema && !CompareNullOrValue(x.Type, "Cdc")).ToList();

        /// <summary>
        /// Inidicates whether it is first in the JoinHierarchy.
        /// </summary>
        public bool IsFirstInJoinHierarchy { get; private set; }

        /// <summary>
        /// Gets the parent <see cref="JoinConfig"/> in the hierarchy.
        /// </summary>
        public JoinConfig? HierarchyParent { get; private set; }

        /// <summary>
        /// Gets the child <see cref="JoinConfig"/> in the hierarchy.
        /// </summary>
        public JoinConfig? HierarchyChild { get; private set; }

        /// <summary>
        /// Gets or sets the indentation index.
        /// </summary>
        public int IndentIndex { get; set; } = 0;

        /// <summary>
        /// Gets the indentation spaces based on the <see cref="IndentIndex"/>.
        /// </summary>
        public int IndentSpaces => IndentIndex * 4;

        /// <summary>
        /// Gets the join <see cref="Type"/> as SQL.
        /// </summary>
        public string JoinTypeSql => Type?.ToUpperInvariant() switch
        {
            "LEFT" => "LEFT OUTER JOIN",
            "RIGHT" => "RIGHT OUTER JOIN",
            "FULL" => "FULL OUTER JOIN",
            _ => "INNER JOIN"
        };

        /// <summary>
        /// Indicates whether there is at least one global identifier being used somewhere.
        /// </summary>
        public bool UsesGlobalIdentifier { get; private set; }

        #endregion

        /// <inheritdoc/>
        protected override async Task PrepareAsync()
        {
            if (Name != null && Name.StartsWith("@", StringComparison.OrdinalIgnoreCase))
                Name = Name[1..];

            Table = DefaultWhereNull(Table, () => Name);
            Schema = DefaultWhereNull(Schema, () => Parent!.Schema);
            DbTable = Root!.DbTables?.Where(x => x.Name == Table && x.Schema == Schema).SingleOrDefault() ?? throw new CodeGenException(this, nameof(Table), $"Specified table '[{Schema}].[{Table}]' not found in database.");
            if (DbTable.IsAView)
                throw new CodeGenException(this, nameof(Table), $"Specified table '[{Schema}].[{Table}]' cannot be a view.");

            Type = DefaultWhereNull(Type, () => "Cdc");
            Model = DefaultWhereNull(Model, () => StringConverter.ToPascalCase(Name));
            JoinTo = DefaultWhereNull(JoinTo, () => Parent!.Table);
            JoinToSchema = DefaultWhereNull(JoinToSchema, () => Parent!.Schema);
            JoinCardinality = DefaultWhereNull(JoinCardinality, () => "OneToMany");
            CdcEnable = DefaultWhereNull(CdcEnable, () => Root.CdcEnable);
            Property = DefaultWhereNull(Property, () => JoinCardinality == "OneToMany" ? StringConverter.ToPlural(Model) : Model);
            if (ExcludeColumnsFromETag == null && Root!.ExcludeColumnsFromETag != null)
                ExcludeColumnsFromETag = new List<string>(Root!.ExcludeColumnsFromETag!);

            if (IsTrue(CdcEnable) && Type! == "Cdc")
                Root.AddCdcEnabled(Schema!, Table!);

            QuerySizeMultiplier = DefaultWhereNull(QuerySizeMultiplier, () => 1.0m);
            if (QuerySizeMultiplier <= 0 || QuerySizeMultiplier > 100)
                QuerySizeMultiplier = 1;

            // Get the JoinTo CdcJoinConfig.
            JoinConfig? jtc = null;
            if (JoinTo != Parent!.Table || JoinToSchema != Parent!.Schema)
            {
                var tables = Parent!.Joins!.Where(x => x.Table == JoinTo && x.Schema == JoinToSchema).ToList();
                if (tables.Count == 0 || Parent!.Joins!.IndexOf(this) < Parent!.Joins!.IndexOf(tables[0]))
                    throw new CodeGenException(this, nameof(JoinTo), $"Specified JoinTo table '[{JoinToSchema}].[{JoinTo}]' must be previously specified.");
                else if (tables.Count > 1)
                    throw new CodeGenException(this, nameof(JoinTo), $"Specified JoinTo table '[{JoinToSchema}].[{JoinTo}]' is ambiguous (more than one found).");

                jtc = tables[0];
                JoinToAlias = tables[0].Alias;
            }
            else
                JoinToAlias = Parent!.Alias;

            // Prepare the identifier mappings.
            Mappings = await PrepareCollectionAsync(Mappings).ConfigureAwait(false);

            if (IncludeColumns != null)
            {
                foreach (var ic in IncludeColumns)
                {
                    if (DbTable.Columns.Where(x => x.Name == ic).SingleOrDefault() == null)
                        throw new CodeGenException(this, nameof(IncludeColumns), $"Specified column '{ic}' not found in table '[{Schema}].[{Table}]'.");
                }
            }

            if (ExcludeColumns != null)
            {
                foreach (var ec in ExcludeColumns)
                {
                    if (DbTable.Columns.Where(x => x.Name == ec).SingleOrDefault() == null)
                        throw new CodeGenException(this, nameof(ExcludeColumns), $"Specified column '{ec}' not found in table '[{Schema}].[{Table}]'.");
                }
            }

            if (AliasColumns != null)
            {
                foreach (var ac in AliasColumns)
                {
                    var parts = ac.Split("^", StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2 || string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]))
                        throw new CodeGenException(this, nameof(AliasColumns), $"Invalid alias column '{ac}' format.");

                    if (DbTable.Columns.Where(x => x.Name == parts[0]).SingleOrDefault() == null)
                        throw new CodeGenException(this, nameof(AliasColumns), $"Specified column '{parts[0]}' not found in table '[{Schema}].[{Table}]'.");
                }
            }

            // Deal with the columns.
            foreach (var c in DbTable.Columns)
            {
                // Handle where primary key.
                JoinColumnConfig? cc = null;
                if (c.IsPrimaryKey)
                {
                    cc = new JoinColumnConfig
                    {
                        Name = c.Name,
                        DbColumn = c,
                        IncludeColumnOnDelete = IncludeColumnsOnDelete != null && IncludeColumnsOnDelete.Contains(c.Name!),
                        IgnoreSerialization = IdentifierMapping == true || (ExcludeColumns != null && ExcludeColumns.Contains(c.Name!))
                    };

                    await cc.PrepareAsync(Root!, this).ConfigureAwait(false);
                    PrimaryKeyColumns.Add(cc);
                }

                // Handle as a standard column.
                cc ??= new JoinColumnConfig { Name = c.Name, DbColumn = c, IncludeColumnOnDelete = IncludeColumnsOnDelete != null && IncludeColumnsOnDelete.Contains(c.Name!) };

                var ca = AliasColumns?.Where(x => x.StartsWith(c.Name + "^", StringComparison.Ordinal)).FirstOrDefault();
                if (ca != null)
                {
                    var parts = ca.Split("^", StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                        cc.NameAlias = parts[1];
                }

                var cm = Mappings.SingleOrDefault(x => x.Name == cc.Name);
                if (cm != null)
                {
                    cc.IdentifierMappingAlias = $"_im{(Mappings.IndexOf(cm) + 1)}";
                    cc.IdentifierMappingSchema = cm.Schema;
                    cc.IdentifierMappingTable = cm.Table;
                }

                await cc.PrepareAsync(Root!, this).ConfigureAwait(false);
                Columns.Add(cc);

                if (cc.IdentifierMappingTable != null)
                {
                    var cc2 = new JoinColumnConfig
                    {
                        Name = "GlobalId",
                        DbColumn = new DbColumnSchema(cc.DbColumn!.DbTable, c.Name, Root!.IdentifierMappingSqlType),
                        NameAlias = "Global" + cc.NameAlias,
                        IdentifierMappingAlias = cc.IdentifierMappingAlias,
                        IdentifierMappingSchema = cc.IdentifierMappingSchema,
                        IdentifierMappingTable = cc.IdentifierMappingTable,
                        IdentifierMappingParent = cc
                    };

                    cc.IdentifierMappingAlias = null;
                    cc.IdentifierMappingParent = null;
                    cc.IgnoreSerialization = true;

                    await cc2.PrepareAsync(Root!, this).ConfigureAwait(false);
                    Columns.Add(cc2);
                }

                UsesGlobalIdentifier = IdentifierMapping == true;
            }

            // Update the Join ons.
            On ??= new List<JoinOnConfig>();

            foreach (var on in On)
            {
                await on.PrepareAsync(Root!, this).ConfigureAwait(false);
            }

            // Manage the Include/Exclude columns.
            if (IncludeColumns != null && IncludeColumns.Count > 0)
            {
                if (Type == "Cdc")
                    Columns.RemoveAll(x => !x.DbColumn!.IsPrimaryKey && !x.IsUsedInJoinOn && !IncludeColumns.Contains(x.Name!));
                else
                    Columns.RemoveAll(x => !IncludeColumns.Contains(x.Name!));

                Columns.Where(x => !IncludeColumns.Contains(x.Name!)).ForEach(x => x.IgnoreSerialization = true);
            }

            if (ExcludeColumns != null && ExcludeColumns.Count > 0)
            {
                if (Type == "Cdc")
                    Columns.RemoveAll(x => !x.DbColumn!.IsPrimaryKey && !x.IsUsedInJoinOn && ExcludeColumns.Contains(x.Name!));
                else
                    Columns.RemoveAll(x => ExcludeColumns.Contains(x.Name!));

                Columns.Where(x => ExcludeColumns.Contains(x.Name!)).ForEach(x => x.IgnoreSerialization = true);
            }

            // Wire up the hierarchy (parent and child).
            var jhp = PartialClone(true, jtc == null ? 0 : jtc.JoinHierarchy.Count, null);
            JoinHierarchy.Add(jhp);

            if (jtc != null)
            {
                jhp = jtc.PartialClone(false, jtc.JoinHierarchy.Count - 1, jhp);
                JoinHierarchyAdd(jhp);
                for (int i = 1; i < jtc.JoinHierarchy.Count; i++)
                {
                    jhp = jtc.JoinHierarchy[i].PartialClone(false, jtc.JoinHierarchy.Count - i, jhp);
                    JoinHierarchyAdd(jhp);
                }
            }

            jhp = null;
            foreach (var jhr in JoinHierarchyReverse)
            {
                jhr.HierarchyChild = jhp;
                jhp = jhr;
            }
        }

        /// <summary>
        /// Adds the join to the hierarchy.
        /// </summary>
        private void JoinHierarchyAdd(JoinConfig jc)
        {
            foreach (var jci in JoinHierarchy)
            {
                if (jc.Name == jci.Name && jc.Schema == jci.Schema && jc.Table == jci.Table)
                    throw new CodeGenException(this, nameof(JoinTo), $"Join table '{jc.Name} [{jc.Schema}].[{jc.Table}]' is self-referencing (within hierarchy) and has resulted in a circular reference.");
            }

            JoinHierarchy.Add(jc);
        }

        /// <summary>
        /// Performs a partial clone.
        /// </summary>
        private JoinConfig PartialClone(bool isFirst, int indentIndex, JoinConfig? hierarchyParent)
        {
            var j = new JoinConfig
            {
                Name = Name,
                Table = Table,
                Schema = Schema,
                Alias = Alias,
                JoinTo = JoinTo,
                JoinToSchema = JoinToSchema,
                JoinToAlias = JoinToAlias,
                JoinCardinality = JoinCardinality,
                Model = Model,
                Property = Property,
                Type = Type,
                IsFirstInJoinHierarchy = isFirst,
                On = new List<JoinOnConfig>(),
                DbTable = DbTable,
                IndentIndex = indentIndex,
                HierarchyParent = hierarchyParent,
                IdentifierMapping = IdentifierMapping,
                Mappings = Mappings
            };

            j.OverrideRootAndParent(Root!, Parent!);

            foreach (var item in On!)
            {
                var jo = new JoinOnConfig
                {
                    Name = item.Name,
                    NameAlias = item.NameAlias,
                    ToColumn = item.ToColumn,
                    ToColumnAlias = item.ToColumnAlias,
                    ToStatement = item.ToStatement,
                    ToDbColumn = item.ToDbColumn
                };

                jo.OverrideRootAndParent(j.Root!, j);
                j.On.Add(jo);
            }

            return j;
        }
    }
}