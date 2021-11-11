﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using Newtonsoft.Json;
using OnRamp;
using OnRamp.Config;
using OnRamp.Database;
using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace NTangle.Config
{
    /// <summary>
    /// Represents a database query configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [CodeGenClass("Cdc", Title = "'Table' object (database-driven)",
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
        [CodeGenProperty("Key", Title = "The name of the primary table.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the default schema name used where not otherwise explicitly specified.
        /// </summary>
        [JsonProperty("schema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The default schema name used where not otherwise explicitly specified.",
            Description = "Defaults to `Root.Schema`.")]
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the `Schema.Table` alias name.
        /// </summary>
        [JsonProperty("alias", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The `Schema.Table` alias name.",
            Description = "Will automatically default where not specified.")]
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
        [JsonProperty("outboxTableName", DefaultValueHandling = DefaultValueHandling.Ignore)]
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
        /// Gets or sets the CDC .NET <see cref="Data.IDatabase"/> interface name.
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
        [CodeGenProperty("CDC", Title = "The type of service that manages the underlying orchestrator.", Options = new string[] { "None", "HostedService" },
            Description = "Defaults to `Root.Service`.")]
        public string? Service { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names that should be excluded from the generated ETag (used for the likes of duplicate send tracking).
        /// </summary>
        [JsonProperty("excludeColumnsFromETag", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection(".NET", Title = "The list of `Column` names that should be excluded from the generated ETag (used for the likes of duplicate send tracking).",
            Description = "Defaults to `Root.CdcExcludeColumnsFromETag`.")]
        public List<string>? ExcludeColumnsFromETag { get; set; }

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

        ///// <summary>
        ///// Gets or sets the list of `Column` with related `Schema`/`Table` values (all split by a `^` lookup character) to enable column one-to-one identifier mapping.
        ///// </summary>
        //[JsonProperty("identifierMappingColumns", DefaultValueHandling = DefaultValueHandling.Ignore)]
        //[CodeGenPropertyCollection("IdentifierMapping", Title = "The list of `Column` with related `Schema`/`Table` values (all split by a `^` lookup character) to enable column one-to-one identifier mapping.", IsImportant = true,
        //    Description = "By default the primary key columns will be automatically selected. Each value is formatted as `Column` + `^` + `Schema` + `^` + `Table` where the schema is optional; e.g. `ContactId^dbo^Contact` or `ContactId^Contact`.")]
        //public List<string>? IdentifierMappingColumns { get; set; }

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
            Markdown = "A `Join` object provides the configuration for a table join.")]
        public List<JoinConfig>? Joins { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="TableIdentifierMappingColumnConfig"/> collection.
        /// </summary>
        [JsonProperty("mappings", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `TableMapping` collection.")]
        public List<TableIdentifierMappingColumnConfig>? Mappings { get; set; }

        #endregion

        #region Non-Config

        /// <summary>
        /// Gets the SQL formatted selected columns.
        /// </summary>
        public List<ColumnConfig> SelectedColumns { get; } = new List<ColumnConfig>();

        /// <summary>
        /// Gets the list of primary key columns.
        /// </summary>
        public List<ColumnConfig> PrimaryKeyColumns { get; } = new List<ColumnConfig>();

        /// <summary>
        /// Gets the SQL formatted selected columns excluding the <see cref="PrimaryKeyColumns"/>.
        /// </summary>
        public List<ColumnConfig> SelectedColumnsExcludingPrimaryKey => SelectedColumns.Where(x => !(x.DbColumn!.DbTable == DbTable && x.DbColumn.IsPrimaryKey)).ToList();

        /// <summary>
        /// Gets the SQL formatted selected columns for the .NET Entity (sans IsDeleted).
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
        /// Gets the <see cref="JoinConfig"/> collection for "all" those that are not flagged as CDC monitored.
        /// </summary>
        public List<JoinConfig> NonCdcJoins => Joins!.Where(x => !CompareNullOrValue(x.Type, "Cdc")).ToList();

        /// <summary>
        /// Gets the list of CDC joined "directly related" children.
        /// </summary>
        public List<JoinConfig> JoinCdcChildren => Joins.Where(x => x.JoinTo == Name && x.JoinToSchema == Schema && CompareNullOrValue(x.Type, "Cdc")).ToList();

        /// <summary>
        /// Gets the list of non-CDC joined "directly related" children.
        /// </summary>
        public List<JoinConfig> JoinNonCdcChildren => Joins.Where(x => x.JoinTo == Name && x.JoinToSchema == Schema && !CompareNullOrValue(x.Type, "Cdc")).ToList();

        /// <summary>
        /// Gets the Orchestrator constructor parameters.
        /// </summary>
        public List<CtorParameterConfig> OrchestratorCtorParameters { get; } = new List<CtorParameterConfig>();

        /// <summary>
        /// Gets the table name.
        /// </summary>
        public string? Table => Name;

        /// <summary>
        /// Gets the corresponding (actual) database table configuration.
        /// </summary>
        public DbTable? DbTable { get; private set; }

        /// <summary>
        /// Gets the related <see cref="IsDeletedColumn"/> column.
        /// </summary>
        public ColumnConfig? ColumnConfigIsDeleted => GetSpecialColumn(IsDeletedColumn);

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
        /// Gets the list of properties to exlcude from the ETag.
        /// </summary>
        public List<string> ExcludePropertiesFromETag { get; set; } = new List<string>();

        #endregion

        /// <inheritdoc/>
        protected override void Prepare()
        {
            Schema = DefaultWhereNull(Schema, () => Root!.Schema);
            DbTable = Root!.DbTables!.Where(x => x.Name == Name && x.Schema == Schema).SingleOrDefault();
            if (DbTable == null)
                throw new CodeGenException(this, nameof(Name), $"Specified table '[{Schema}].[{Name}]' not found in database.");

            if (DbTable.IsAView)
                throw new CodeGenException(this, nameof(Name), $"Specified table '[{Schema}].[{Name}]' cannot be a view.");

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
            Database = DefaultWhereNull(Database, () => "IDatabase");
            Service = DefaultWhereNull(Service, () => Root.Service);
            IsDeletedColumn = DefaultWhereNull(IsDeletedColumn, () => Root!.IsDeletedColumn);
            if (ExcludeColumnsFromETag == null && Root!.ExcludeColumnsFromETag != null)
                ExcludeColumnsFromETag = new List<string>(Root!.ExcludeColumnsFromETag!);

            if (IsTrue(CdcEnable))
                Root.AddCdcEnabled(Schema!, Table!);

            Mappings = PrepareCollection(Mappings);

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
                    cc.Prepare(Root!, this);
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
                            cc.IdentifierMappingAlias = $"_im{(Mappings.IndexOf(cm) + 1)}";
                            cc.IdentifierMappingSchema = cm.Schema;
                            cc.IdentifierMappingTable = cm.Table;
                        }

                        cc.Prepare(Root!, this);
                        Columns.Add(cc);
                    }
                }

                // Always include IsDeleted!
                if (cc.Name == ColumnConfigIsDeleted?.Name)
                {
                    cc.NameAlias = "IsDeleted";
                    cc.Prepare(Root!, this);
                    Columns.Add(cc);
                }
            }

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

                cc.Prepare(Root!, this);
                SelectedColumns.Add(cc);

                if (c.IdentifierMappingTable != null)
                {
                    cc = new ColumnConfig
                    {
                        Name = "GlobalId",
                        DbColumn = new DbColumn { Name = c.Name, Type = Root!.IdentifierMappingSqlType, DbTable = c.DbColumn!.DbTable },
                        NameAlias = "Global" + c.NameAlias,
                        IdentifierMappingAlias = c.IdentifierMappingAlias,
                        IdentifierMappingSchema = c.IdentifierMappingSchema,
                        IdentifierMappingTable = c.IdentifierMappingTable,
                        IdentifierMappingParent = cc
                    };

                    cc.Prepare(Root!, this);
                    SelectedColumns.Add(cc);
                }
            }

            PrepareCtorParams();
            PrepareJoins();

            UsesGlobalIdentifier = IdentifierMapping == true || Mappings!.Count > 0 || Joins.Any(x => x.IdentifierMapping == true || (x.Mappings!.Count > 0));
            SetUpExcludePropertiesFromETag();
        }

        /// <summary>
        /// Prepares the constructor parameters.
        /// </summary>
        private void PrepareCtorParams()
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

                pc.Prepare(Root!, this);
                OrchestratorCtorParameters.Add(pc);
            }
        }

        /// <summary>
        /// Prepares the joins.
        /// </summary>
        private void PrepareJoins()
        {
            if (Joins == null)
                Joins = new List<JoinConfig>();

            // Prepare the Join and also make sure the alias is unique.
            var dict = new Dictionary<string, int> { { Alias!, 1 } };
            foreach (var join in Joins)
            {
                join.Alias = DefaultWhereNull(join.Alias, () => DbTable.CreateAlias(join.Name!));

                if (dict.TryGetValue(join.Alias!, out var val))
                {
                    dict[join.Alias!] = ++val;
                    join.Alias = $"{join.Alias}{val}";
                }
                else
                    dict.Add(join.Alias!, 1);

                join.Prepare(Root!, this);
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
        private ColumnConfig? GetSpecialColumn(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var c = DbTable!.Columns.Where(x => x.Name == name && !x.IsPrimaryKey).SingleOrDefault();
            if (c == null)
                return null;

            var cc = new ColumnConfig { Name = c.Name, DbColumn = c };
            cc.Prepare(Root!, this);
            return cc;
        }
    }
}