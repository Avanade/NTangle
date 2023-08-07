// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using Newtonsoft.Json;
using OnRamp;
using OnRamp.Config;
using DbEx.DbSchema;
using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace NTangle.Config
{
    /// <summary>
    /// Represents a database query configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [CodeGenClass("Table", Title = "'Table' object (database-driven)",
        Description = "The `Table` object enables the definition of the primary table, one-or-more child tables and their respective join relationships, to enable Change Data Capture (CDC) event publishing."
            + " The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude selected columns; with the `AliasColumns` providing a means to rename where required.",
        ExampleMarkdown = @"A YAML configuration example is as follows:
``` yaml
```")]
    [CodeGenCategory("Key", Title = "Provides the _key_ configuration.")]
    [CodeGenCategory("Columns", Title = "Provides the _Columns_ configuration.")]
    [CodeGenCategory("Database", Title = "Provides the _database_ configuration.")]
    [CodeGenCategory(".NET", Title = "Provides the _.NET_ configuration.")]
    [CodeGenCategory("Event", Title = "Provides the _event_ configuration.")]
    [CodeGenCategory("Infer", Title = "Provides the _special Column Name inference_ configuration.")]
    [CodeGenCategory("IdentifierMapping", Title = "Provides the _identifier mapping_ configuration.")]
    [CodeGenCategory("Collections", Title = "Provides related child (hierarchical) configuration.")]
    public class TableConfig : ConfigBase<RootConfig, RootConfig>, ITableReference
    {
        /// <inheritdoc/>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("Table", Name);

        #region Key

        /// <summary>
        /// Gets or sets the name of the primary table.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The name of the primary table.",  IsMandatory = true, IsImportant = true, IsUnique = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the default schema name used where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("schema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The default schema name used where not otherwise explicitly specified.",
            Description = "Defaults to `Root.Schema`.")]
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the name of the primary table.
        /// </summary>
        [JsonProperty("table", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The name of the primary table.",
            Description = "Defaults to `Name`. This is used to specify the actual underlying database table name (required where the `Name` has been changed to enable uniqueness).")]
        public string? Table { get; set; }

        /// <summary>
        /// Gets or sets the table alias name.
        /// </summary>
        [JsonProperty("alias", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The table alias name (must be unique).",
            Description = "Will automatically default where not specified; for example a table named `Person` will default to `p`.")]
        public string? Alias { get; set; }

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
        [CodeGenPropertyCollection("Columns", Title = "The list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column aliasing/renaming.", IsImportant = true,
            Description = "Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`.")]
        public List<string>? AliasColumns { get; set; }

        #endregion

        #region Database

        /// <summary>
        /// Gets or sets the `Cdc` execute batch stored procedure name.
        /// </summary>
        [JsonProperty("executeStoredProcedure", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Database", Title = "The `CDC` _execute_ batch stored procedure name.",
            Description = "Defaults to `sp` (literal) + `Name` + `BatchExecute` (literal); e.g. `spNameBatchExecute`.")]
        public string? ExecuteStoredProcedure { get; set; }

        /// <summary>
        /// Gets or sets the `Cdc` complete batch stored procedure name.
        /// </summary>
        [JsonProperty("completeStoredProcedure", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Database", Title = "The `CDC` _complete_ batch stored procedure name.",
            Description = "Defaults to `sp` (literal) + `Name` + `BatchComplete` (literal); e.g. `spNameBatchComplete`.")]
        public string? CompleteStoredProcedure { get; set; }

        /// <summary>
        /// Gets or sets the schema name for the `Cdc`-related database artefacts.
        /// </summary>
        [JsonProperty("cdcSchema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Database", Title = "The schema name for the generated `CDC`-related database artefacts.",
            Description = "Defaults to `Root.CdcSchema`.")]
        public string? CdcSchema { get; set; }

        /// <summary>
        /// Gets or sets the corresponding `Cdc` Batch tracking table name.
        /// </summary>
        [JsonProperty("batchTrackingTable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Database", Title = "The corresponding `CDC` Batch tracking table name.",
            Description = "Defaults to `Name` + `BatchTracking` (literal).")]
        public string? BatchTrackingTable { get; set; }

        /// <summary>
        /// Indicates whether to enable `Cdc` within the database for the tables that participate.
        /// </summary>
        [JsonProperty("cdcEnable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Database", Title = "Indicates whether to enable `Cdc` within the database for the tables that participate.",
            Description = "Defaults to `false`. This option can be overridden for each underlying table referenced.")]
        public bool? CdcEnable { get; set; }

        #endregion

        #region .NET

        /// <summary>
        /// Gets or sets the `Cdc` .NET model name.
        /// </summary>
        [JsonProperty("model", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty(".NET", Title = "The .NET model name.",
            Description = "Defaults to `Name`.")]
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the list of extended (non-default) Dependency Injection (DI) parameters for the generated CDC `Orchestrator` constructor.
        /// </summary>
        [JsonProperty("orchestratorCtorParams", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection(".NET", Title = "The list of additional (non-default) Dependency Injection (DI) parameters for the generated CDC `Orchestrator` constructor.",
            Description = "Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred.")]
        public List<string>? OrchestratorCtorParams { get; set; }

        /// <summary>
        /// Gets or sets the CDC .NET <see cref="CoreEx.Database.IDatabase"/> interface name.
        /// </summary>
        [JsonProperty("database", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty(".NET", Title = "The .NET database `IDatabase` Type name used in the constructor for Dependency Injection (DI).",
            Description = "Defaults to `IDatabase`.")]
        public string? Database { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names that should be included (in addition to the primary key) for a logical delete.
        /// </summary>
        [JsonProperty("includeColumnsOnDelete", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection(".NET", Title = "The list of `Column` names that should be included (in addition to the primary key) for a logical delete.",
           Description = "Where a column is not specified in this list its corresponding .NET property will be automatically cleared by the `CdcDataOrchestrator` as the data is technically considered as non-existing.")]
        public List<string>? IncludeColumnsOnDelete { get; set; }

        /// <summary>
        /// Gets or sets the type of service that manages the underlying orchestrator.
        /// </summary>
        [JsonProperty("service", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("CDC", Title = "The type of service that manages the underlying orchestrator.", Options = new string[] { "None", "HostedService", "Service" },
            Description = "Defaults to `Root.Service`. A `HostedService` is an `IHostedService` implementation enabling long-running execution; whereas, `Service` is intended for self-managed execution.")]
        public string? Service { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names that should be excluded from the generated ETag (used for the likes of duplicate send tracking).
        /// </summary>
        [JsonProperty("excludeColumnsFromETag", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection(".NET", Title = "The list of `Column` names that should be excluded from the generated ETag (used for the likes of duplicate send tracking).",
            Description = "Defaults to `Root.CdcExcludeColumnsFromETag`.")]
        public List<string>? ExcludeColumnsFromETag { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names that represent the tenant id.
        /// </summary>
        [JsonProperty("tenantIdColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection(".NET", Title = "The list of `Column` names that represent the tenant identifier.")]
        public List<string>? TenantIdColumns { get; set; }

        /// <summary>
        /// Gets or sets the partition key.
        /// </summary>
        [JsonProperty("partitionKey", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty(".NET", Title = "The partition key.",
            Description = "A partition key can be specified using either `PartitionKey` or `PartitionKeyColumns`.")]
        public string? PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names that represent the partition key.
        /// </summary>
        [JsonProperty("partitionKeyColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection(".NET", Title = "The list of `Column` names that represent the partition key.",
            Description = "A partition key can be specified using either `PartitionKey` or `PartitionKeyColumns`.")]
        public List<string>? PartitionKeyColumns { get; set; }

        #endregion

        #region Event

        /// <summary>
        /// Gets or sets the URI event source.
        /// </summary>
        [JsonProperty("eventSource", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Event", Title = "The Event Source.",
            Description = "Defaults to `Schema` + `/` (literal) + `Name` (as lowercase). Note: when used in code-generation the `Root.EventSourceRoot` will be prepended where specified.")]
        public string? EventSource { get; set; }

        /// <summary>
        /// Gets or sets the default formatting for the Source when an Event is published.
        /// </summary>
        [JsonProperty("eventSourceFormat", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Event", Title = "The default formatting for the Source when an Event is published.", Options = new string[] { "NameOnly", "NameAndKey", "NameAndTableKey" },
            Description = "Defaults to `Root.EventSourceFormat`.")]
        public string? EventSourceFormat { get; set; }

        /// <summary>
        /// Gets or sets the event subject.
        /// </summary>
        [JsonProperty("eventSubject", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Event", Title = "The Event Subject.",
            Description = "Defaults to `ModelName`. Note: when used in code-generation the `Root.EventSubjectRoot` will be prepended where specified.")]
        public string? EventSubject { get; set; }

        /// <summary>
        /// Gets or sets the default formatting for the Subject when an Event is published.
        /// </summary>
        [JsonProperty("eventSubjectFormat", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Event", Title = "The default formatting for the Subject when an Event is published.", Options = new string[] { "NameOnly", "NameAndKey", "NameAndTableKey" },
            Description = "Defaults to `Root.EventSubjectFormat`.")]
        public string? EventSubjectFormat { get; set; }

        /// <summary>
        /// Gets or sets the event type.
        /// </summary>
        [JsonProperty("eventType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Event", Title = "The Event Type.",
            Description = "Defaults to `ModelName`. Note: when used in code-generation the `Root.EventTypeRoot` will be prepended where specified.")]
        public string? EventType { get; set; }

        #endregion

        #region IdentifierMapping

        /// <summary>
        /// Indicates whether to perform Identifier Mapping (mapping to `GlobalId`) for the primary key.
        /// </summary>
        [JsonProperty("identifierMapping", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("IdentifierMapping", Title = "Indicates whether to perform Identifier Mapping (mapping to `GlobalId`) for the primary key.", IsImportant = true,
           Description = "This indicates whether to create a new `GlobalId` property on the _entity_ to house the global mapping identifier to be the reference outside of the specific database realm as a replacement to the existing primary key column(s)." 
            + " Defaults to `Root.IdentifierMapping`.")]
        public bool? IdentifierMapping { get; set; }

        #endregion

        #region Infer

        /// <summary>
        /// Gets or sets the column name for the `IsDeleted` capability.
        /// </summary>
        [JsonProperty("isDeletedColumn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `IsDeleted` (logical delete) capability (if any).",
            Description = "Defaults to `Root.ColumnIsDeleted`.")]
        public string? IsDeletedColumn { get; set; }

        #endregion

        #region Collections

        /// <summary>
        /// Gets or sets the corresponding <see cref="JoinConfig"/> collection.
        /// </summary>
        [JsonProperty("joins", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `Join` collection.", IsImportant = true,
            Description = "A `Join` object provides the configuration for a table join.")]
        public List<JoinConfig>? Joins { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="WhereConfig"/> collection.
        /// </summary>
        [JsonProperty("where", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `Where` collection.", IsImportant = true,
            Description = "A `Where` object provides the configuration for a table where clause.")]
        public List<WhereConfig>? Where { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="TableIdentifierMappingColumnConfig"/> collection.
        /// </summary>
        [JsonProperty("mappings", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `TableMapping` collection.")]
        public List<TableIdentifierMappingColumnConfig>? Mappings { get; set; }

        #endregion

        #region Non-Config

        /// <summary>
        /// Gets the selected columns.
        /// </summary>
        public List<ColumnConfig> SelectedColumns { get; } = new List<ColumnConfig>();

        /// <summary>
        /// Gets the list of primary key columns.
        /// </summary>
        public List<ColumnConfig> PrimaryKeyColumns { get; } = new List<ColumnConfig>();

        /// <summary>
        /// Gets the selected columns excluding the <see cref="PrimaryKeyColumns"/>.
        /// </summary>
        public List<ColumnConfig> SelectedColumnsExcludingPrimaryKey => SelectedColumns.Where(x => !(x.DbColumn!.DbTable == DbTable && x.DbColumn.IsPrimaryKey)).ToList();

        /// <summary>
        /// Gets the selected columns for the .NET Entity (sans IsDeleted).
        /// </summary>
        public List<ColumnConfig> SelectedEntityColumns => SelectedColumns.Where(x => !x.IsIsDeletedColumn).ToList();

        /// <summary>
        /// Gets the selected column configurations.
        /// </summary>
        public List<ColumnConfig> Columns { get; } = new List<ColumnConfig>();

        /// <summary>
        /// Gets the <see cref="JoinConfig"/> collection for "all" those that are also CDC monitored.
        /// </summary>
        public List<JoinConfig> CdcJoins => Joins!.Where(x => CompareNullOrValue(x.Type, "Cdc")).ToList();

        /// <summary>
        /// Gets the <see cref="DistinctModels"/> collection for distinct CdcJoins.
        /// </summary>
        public List<JoinConfig> DistinctModels => CdcJoins.GroupBy(e=>e.Model).Select(p=>p.First()).ToList();

        /// <summary>
        /// Gets the <see cref="JoinConfig"/> collection for "all" those that are not flagged as CDC monitored.
        /// </summary>
        public List<JoinConfig> NonCdcJoins => Joins!.Where(x => !CompareNullOrValue(x.Type, "Cdc")).ToList();

        /// <summary>
        /// Gets the list of CDC joined "directly related" children.
        /// </summary>
        public List<JoinConfig> JoinCdcChildren => Joins!.Where(x => x.JoinToTable == Table && x.JoinToSchema == Schema && CompareNullOrValue(x.Type, "Cdc")).ToList();

        /// <summary>
        /// Gets the list of non-CDC joined "directly related" children.
        /// </summary>
        public List<JoinConfig> JoinNonCdcChildren => Joins!.Where(x => x.JoinToTable == Table && x.JoinToSchema == Schema && !CompareNullOrValue(x.Type, "Cdc")).ToList();

        /// <summary>
        /// Gets the Orchestrator constructor parameters.
        /// </summary>
        public List<CtorParameterConfig> OrchestratorCtorParameters { get; } = new List<CtorParameterConfig>();

        /// <summary>
        /// Gets the corresponding (actual) database table configuration.
        /// </summary>
        public DbTableSchema? DbTable { get; private set; }

        /// <summary>
        /// Gets the related <see cref="IsDeletedColumn"/> column.
        /// </summary>
        public ColumnConfig? ColumnConfigIsDeleted { get; private set; }

        /// <summary>
        /// Gets the fully qualified name schema.table name.
        /// </summary>
        public string? QualifiedName => DbTable!.QualifiedName;

        /// <summary>
        /// Gets the event source URI.
        /// </summary>
        public string EventSourceUri => Root!.EventSourceRoot + (EventSource!.StartsWith('/') || (Root!.EventSourceRoot != null && Root!.EventSourceRoot.EndsWith('/')) ? EventSource : ("/" + EventSource));

        /// <summary>
        /// Indicates whether there is at least one global identifier being used somewhere.
        /// </summary>
        public bool UsesGlobalIdentifier { get; private set; }

        /// <summary>
        /// Gets the identifier column - single primary key column with alias of 'ID'.
        /// </summary>
        public ColumnConfig? IdentifierColumn { get; private set; }

        /// <summary>
        /// Gets the list of properties to exlcude from the ETag.
        /// </summary>
        public List<string> ExcludePropertiesFromETag { get; set; } = new List<string>();

        /// <summary>
        /// Gets the selected tenant identitifer columns.
        /// </summary>
        public List<ColumnConfig> SelectedTenantIdColumns { get; } = new List<ColumnConfig>();

        /// <summary>
        /// Gets the selected partition key columns.
        /// </summary>
        public List<ColumnConfig> SelectedPartitionKeyColumns { get; } = new List<ColumnConfig>();

        /// <summary>
        /// Indicates whether the partition key has been specified.
        /// </summary>
        public bool HasPartitionKey { get; set; }

        #endregion

        /// <inheritdoc/>
        protected override async Task PrepareAsync()
        {
            Schema = DefaultWhereNull(Schema, () => Root!.Schema);
            Table = DefaultWhereNull(Table, () => Name);
            DbTable = Root!.DbTables!.Where(x => x.Name == Table && x.Schema == Schema).SingleOrDefault();
            if (DbTable == null)
                throw new CodeGenException(this, nameof(Table), $"Specified table '[{Schema}].[{Table}]' not found in database.");

            if (DbTable.IsAView)
                throw new CodeGenException(this, nameof(Table), $"Specified table '[{Schema}].[{Table}]' cannot be a view.");

            Alias = DefaultWhereNull(Alias, () => DbTable.Alias);

            ExecuteStoredProcedure = DefaultWhereNull(ExecuteStoredProcedure, () => $"sp{StringConverter.ToPascalCase(Name)}BatchExecute");
            CompleteStoredProcedure = DefaultWhereNull(CompleteStoredProcedure, () => $"sp{StringConverter.ToPascalCase(Name)}BatchComplete");
            CdcSchema = DefaultWhereNull(CdcSchema, () => Root.CdcSchema);
            CdcEnable = DefaultWhereNull(CdcEnable, () => Root.CdcEnable);
            BatchTrackingTable = DefaultWhereNull(BatchTrackingTable, () => Name + "BatchTracking");
            Model = DefaultWhereNull(Model, () => Root.RenameForDotNet(Name));
            EventSource = DefaultWhereNull(EventSource, () => $"{Schema!.ToLowerInvariant()}/{Table!.ToLowerInvariant()}");
            EventSourceFormat = DefaultWhereNull(EventSourceFormat, () => Root!.EventSourceFormat);
            EventSubject = DefaultWhereNull(EventSubject, () => Model);
            EventSubjectFormat = DefaultWhereNull(EventSubjectFormat, () => Root!.EventSubjectFormat);
            EventType = DefaultWhereNull(EventType, () => Model);
            Database = DefaultWhereNull(Database, () => "IDatabase");
            Service = DefaultWhereNull(Service, () => Root.Service);
            IsDeletedColumn = DefaultWhereNull(IsDeletedColumn, () => Root!.IsDeletedColumn);
            if (ExcludeColumnsFromETag == null && Root!.ExcludeColumnsFromETag != null)
                ExcludeColumnsFromETag = new List<string>(Root!.ExcludeColumnsFromETag!);

            if (IsTrue(CdcEnable))
                Root.AddCdcEnabled(Schema!, Table!);

            Where = await PrepareCollectionAsync(Where).ConfigureAwait(false);
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

            foreach (var c in DbTable.Columns)
            {
                var cc = new ColumnConfig { Name = c.Name, DbColumn = c };
                var ca = AliasColumns?.Where(x => x.StartsWith(c.Name + "^", StringComparison.Ordinal)).FirstOrDefault();
                if (ca != null)
                {
                    var parts = ca.Split("^", StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                        cc.NameAlias = parts[1];
                }

                if (c.IsPrimaryKey)
                {
                    cc.IncludeColumnOnDelete = true;
                    cc.IgnoreSerialization = IdentifierMapping == true;
                    await cc.PrepareAsync(Root!, this).ConfigureAwait(false);
                    PrimaryKeyColumns.Add(cc);
                }
                else if (IncludeColumnsOnDelete != null && IncludeColumnsOnDelete.Contains(c.Name!))
                    cc.IncludeColumnOnDelete = true;

                if ((ExcludeColumns == null || !ExcludeColumns.Contains(c.Name!)) && (IncludeColumns == null || IncludeColumns.Contains(c.Name!)))
                {
                    if (cc.Name != ColumnConfigIsDeleted?.Name)
                    {
                        var cm = Mappings.SingleOrDefault(x => x.Name == cc.Name);
                        if (cm != null)
                        {
                            cc.IdentifierMappingAlias = $"_im{Mappings.IndexOf(cm) + 1}";
                            cc.IdentifierMappingSchema = cm.Schema;
                            cc.IdentifierMappingTable = cm.Table;
                        }
                    }
                }
                else if (cc.Name == ColumnConfigIsDeleted?.Name)
                {
                    // Always include IsDeleted!
                    cc.IsIsDeletedColumn = true;
                    cc.NameAlias = "IsDeleted";
                }
                else
                {
                    // Mark for exclusion.
                    cc.IsExcluded = true;
                    cc.IgnoreSerialization = true;
                }

                await cc.PrepareAsync(Root!, this).ConfigureAwait(false);
                Columns.Add(cc);
            }

            await PrepareCtorParamsAsync().ConfigureAwait(false);
            await PrepareJoinsAsync().ConfigureAwait(false);

            Columns.RemoveAll(x => x.IsExcluded && !x.IsIsDeletedColumn && !x.IsUsedInJoinOn);

            // Build up the selected columns list.
            foreach (var c in Columns)
            {
                var cc = new ColumnConfig
                {
                    Name = c.Name,
                    DbColumn = c.DbColumn,
                    NameAlias = c.NameAlias,
                    IncludeColumnOnDelete = c.IncludeColumnOnDelete,
                    IgnoreSerialization = c.IgnoreSerialization || c.IdentifierMappingTable != null
                };

                await cc.PrepareAsync(Root!, this).ConfigureAwait(false);
                SelectedColumns.Add(cc);

                if (c.IdentifierMappingTable != null)
                {
                    cc = new ColumnConfig
                    {
                        Name = "GlobalId",
                        DbColumn = new DbColumnSchema(c.DbColumn!.DbTable, c.Name!, Root!.IdentifierMappingSqlType),
                        NameAlias = "Global" + c.NameAlias,
                        IdentifierMappingAlias = c.IdentifierMappingAlias,
                        IdentifierMappingSchema = c.IdentifierMappingSchema,
                        IdentifierMappingTable = c.IdentifierMappingTable,
                        IdentifierMappingParent = cc
                    };

                    await cc.PrepareAsync(Root!, this).ConfigureAwait(false);
                    SelectedColumns.Add(cc);
                }
            }

            if (PrimaryKeyColumns.Count == 1 && PrimaryKeyColumns[0].NameAlias == "Id")
                IdentifierColumn = PrimaryKeyColumns[0];

            UsesGlobalIdentifier = IdentifierMapping == true || Mappings!.Count > 0 || Joins!.Any(x => x.IdentifierMapping == true || (x.Mappings!.Count > 0));
            SetUpExcludePropertiesFromETag();

            ColumnConfigIsDeleted = await GetSpecialColumnAsync(IsDeletedColumn).ConfigureAwait(false);

            if (TenantIdColumns != null)
            {
                foreach (var cn in TenantIdColumns)
                {
                    var col = SelectedColumns.Where(x => x.Name == cn).FirstOrDefault() ?? throw new CodeGenException(this, nameof(TenantIdColumns), $"TenantId column '[{cn}]' must be a _selected_ column within table '[{Schema}].[{Name}]'.");
                    SelectedTenantIdColumns.Add(col);
                }
            }

            if (PartitionKey != null && PartitionKeyColumns != null && PartitionKeyColumns.Count > 0)
                throw new CodeGenException(this, nameof(PartitionKey), $"PartitionKey and PartitionKeyColumns can not both be specified at the same time.");

            if (PartitionKey != null)
                HasPartitionKey = true;
            else if (PartitionKeyColumns != null)
            {
                foreach (var cn in PartitionKeyColumns)
                {
                    var col = SelectedColumns.Where(x => x.Name == cn).FirstOrDefault() ?? throw new CodeGenException(this, nameof(PartitionKeyColumns), $"PartitionKey column '[{cn}]' must be a _selected_ column within table '[{Schema}].[{Name}]'.");
                    SelectedPartitionKeyColumns.Add(col);
                    HasPartitionKey = true;
                }
            }
        }

        /// <summary>
        /// Prepares the constructor parameters.
        /// </summary>
        private async Task PrepareCtorParamsAsync()
        {
            if (OrchestratorCtorParams == null || OrchestratorCtorParams.Count == 0)
                return;

            foreach (var text in OrchestratorCtorParams)
            {
                var parts = text.Split("^", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    continue;

                var pc = new CtorParameterConfig { Type = parts[0], Text = $"The {StringConverter.ToSeeComments(Name)}." };
                if (parts.Length == 1)
                {
                    var nsparts = parts[0].Split(".", StringSplitOptions.RemoveEmptyEntries);
                    pc.Name = nsparts.Last().Replace("<", "", StringComparison.InvariantCulture).Replace(">", "", StringComparison.InvariantCulture);
                    if (pc.Name[0] == 'I' && pc.Name.Length > 1 && char.IsUpper(pc.Name[1]))
                        pc.Name = pc.Name[1..];
                }
                else
                    pc.Name = StringConverter.ToPascalCase(parts[1]);

                await pc.PrepareAsync(Root!, this).ConfigureAwait(false);
                OrchestratorCtorParameters.Add(pc);
            }
        }

        /// <summary>
        /// Prepares the joins.
        /// </summary>
        private async Task PrepareJoinsAsync()
        {
            Joins ??= new List<JoinConfig>();

            // Prepare the Join and also make sure the alias is unique.
            var dict = new Dictionary<string, int> { { Alias!, 1 } };
            foreach (var join in Joins)
            {
                join.Alias = DefaultWhereNull(join.Alias, () => DbTableSchema.CreateAlias(join.Name!));

                if (dict.TryGetValue(join.Alias!, out var val))
                {
                    dict[join.Alias!] = ++val;
                    join.Alias = $"{join.Alias}{val}";
                }
                else
                    dict.Add(join.Alias!, 1);

                await join.PrepareAsync(Root!, this).ConfigureAwait(false);
            }

            // Do some further validation.
            if (Joins.Any(x => x.Name == Name))
                throw new CodeGenException(this, nameof(Name), $"The Name '{Name}' is ambiguous (not unique); please make 'Name' unique and set 'Table' to the actual table name to correct.");

            foreach (var j in Joins)
            {
                if (Joins.Any(x => x != j && x.Name == j.Name))
                    throw new CodeGenException(this, nameof(Joins), $"The Name '{j.Name}' is ambiguous (not unique); please make 'Name' unique and set 'Table' to the actual table name to correct.");
            }
        }

        /// <summary>
        /// Sets up the <see cref="ExcludePropertiesFromETag"/> list.
        /// </summary>
        private void SetUpExcludePropertiesFromETag()
        {
            if (ExcludeColumnsFromETag != null)
            {
                foreach (var ec in ExcludeColumnsFromETag)
                {
                    var c = Columns.Where(x => x.Name == ec).FirstOrDefault();
                    if (c != null)
                        ExcludePropertiesFromETag.Add(c.NameAlias!);
                }
            }

            if (Joins != null)
            {
                foreach (var j in Joins)
                {
                    if (j.ExcludeColumnsFromETag != null)
                    {
                        var p = string.Join('.', j.JoinHierarchyReverse.Select(x => x.Property));
                        foreach (var ec in j.ExcludeColumnsFromETag)
                        {
                            var c = j.Columns.Where(x => x.Name == ec).FirstOrDefault();
                            if (c != null)
                                ExcludePropertiesFromETag.Add(p + '.' + c.NameAlias!);
                        }
                    }
                }
            }

            if (ExcludePropertiesFromETag != null && ExcludePropertiesFromETag.Count > 0)
                ExcludePropertiesFromETag = ExcludePropertiesFromETag.Distinct().ToList();
        }

        /// <summary>
        /// Gets the named special column.
        /// </summary>
        private async Task<ColumnConfig?> GetSpecialColumnAsync(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var c = DbTable!.Columns.Where(x => x.Name == name && !x.IsPrimaryKey).SingleOrDefault();
            if (c == null)
                return null;

            var cc = new ColumnConfig { Name = c.Name, DbColumn = c };
            await cc.PrepareAsync(Root!, this).ConfigureAwait(false);
            return cc;
        }
    }
}