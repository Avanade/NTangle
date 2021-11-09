// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NTangle.Console;
using OnRamp;
using OnRamp.Config;
using OnRamp.Database;
using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NTangle.Config
{
    /// <summary>
    /// Represents the global database code-generation configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [CodeGenClass("Root", Title = "The 'Root' object (database-driven)",
        Description = "The `CdcRootConfig` object defines the global properties that are used to drive the underlying database-driven CDC-oriented code-generation.",
        ExampleMarkdown = @"A YAML configuration example is as follows:
``` yaml
```")]
    [CodeGenCategory("Key", Title = "Provides the _key_ configuration.")]
    [CodeGenCategory("Identifier", Title = "Provides the _Identifier_ configuration.")]
    [CodeGenCategory("Infer", Title = "Provides the _special Column Name inference_ configuration.")]
    [CodeGenCategory(".NET", Title = "Provides the _.NET_ configuration.")]
    [CodeGenCategory("Event", Title = "Provides the _event_ configuration.")]
    [CodeGenCategory("Path", Title = "Provides the _Path (Directory)_ configuration for the generated artefacts.")]
    [CodeGenCategory("Namespace", Title = "Provides the _.NET Namespace_ configuration for the generated artefacts.")]
    [CodeGenCategory("Collections", Title = "Provides related child (hierarchical) configuration.")]
    public sealed class RootConfig : ConfigRootBase<RootConfig>
    {
        #region Key

        /// <summary>
        /// Gets or sets the default `Schema` name of the where the existing tables are defined in the database.
        /// </summary>
        [JsonProperty("schema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The default `Schema` name where the existing tables are defined within the database.", IsImportant = true,
            Description = "This is used as the default `Schema` for all child objects. Defaults to `dbo` (literal).")]
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the schema name for the `Cdc`-related database artefacts.
        /// </summary>
        [JsonProperty("cdcSchema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The schema name for the _ntangle_ generated `CDC`-related database artefacts.",
            Description = "Defaults to `NTangle` (literal).")]
        public string? CdcSchema { get; set; }

        /// <summary>
        /// Indicates whether to create the `Cdc`-Schema within the database.
        /// </summary>
        [JsonProperty("cdcSchemaCreate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "Indicates whether to create the CDC Schema within the database.",
            Description = "Defaults to `false`.")]
        public bool? CdcSchemaCreate { get; set; }

        /// <summary>
        /// Gets or sets the table name for the `Cdc`-VersionTracking.
        /// </summary>
        [JsonProperty("versionTrackingTable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The table name for the `Cdc`-VersionTracking.",
            Description = "Defaults to `VersionTracking` (literal).")]
        public string? VersionTrackingTable { get; set; }

        /// <summary>
        /// Indicates whether to enable `Cdc` within the database for the tables that participate.
        /// </summary>
        [JsonProperty("cdcEnable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "Indicates whether to enable `Cdc` within the database for the tables that participate.",
            Description = "Defaults to `false`. This option can be overridden for each underlying table referenced.")]
        public bool? CdcEnable { get; set; }

        #endregion

        #region Identifier

        /// <summary>
        /// Indicates whether to include the generation of the generic `CDC`-IdentifierMapping database capabilities.
        /// </summary>
        [JsonProperty("identifierMapping", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Identifier", Title = "Indicates whether to include the generation of the generic `CDC`-IdentifierMapping database capabilities.",
            Description = "Where set to `true` each underlying `Table` and corresponding `Join` must set `IdentifierMapping` explicitly.")]
        public bool? IdentifierMapping { get; set; }

        /// <summary>
        /// Gets or sets the identifier mapping <see cref="Type"/>.
        /// </summary>
        [JsonProperty("identifierMappingType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Identifier", Title = "The type for the identifier mapping value.", Options = new string[] { "String", "Int", "Long", "Guid" },
            Description = "Defaults to `String`.")]
        public string? IdentifierMappingType { get; set; }

        /// <summary>
        /// Gets or sets the table name for the `Cdc`-IdentifierMapping.
        /// </summary>
        [JsonProperty("identifierMappingTable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Identifier", Title = "The table name for the `Cdc`-IdentifierMapping.",
            Description = "Defaults to `IdentifierMapping` (literal).")]
        public string? IdentifierMappingTable { get; set; }

        /// <summary>
        /// Gets or sets the stored procedure name for the `Cdc`-IdentifierMapping create.
        /// </summary>
        [JsonProperty("identifierMappingStoredProcedure", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Identifier", Title = "The stored procedure name for the `Cdc`-IdentifierMapping create.",
            Description = "Defaults to `spIdentifierMappingCreate` (literal).")]
        public string? IdentifierMappingStoredProcedure { get; set; }

        #endregion

        #region Infer

        /// <summary>
        /// Gets or sets the column name for the `IsDeleted` capability.
        /// </summary>
        [JsonProperty("columnIsDeleted", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Infer", Title = "The column name for the `IsDeleted` (logical delete) capability (if any).",
            Description = "Defaults to `IsDeleted`.")]
        public string? ColumnIsDeleted { get; set; }

        #endregion

        #region .NET

        /// <summary>
        /// Gets or sets the option to automatically rename the SQL Tables and Columns for use in .NET.
        /// </summary>
        [JsonProperty("autoDotNetRename", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty(".NET", Title = "The option to automatically rename the SQL Tables and Columns for use in .NET.", Options = new string[] { "None", "PascalCase", "SnakeKebabToPascalCase" },
            Description = "Defaults to `SnakeKebabToPascalCase` which will remove any underscores or hyphens separating each word and capitalize the first character of each; e.g. `internal-customer_id` would be renamed as `InternalCustomerId`. The `PascalCase` option will capatilize the first character only.")]
        public string? AutoDotNetRename { get; set; }

        /// <summary>
        /// Gets or sets the default list of `Column` names that should be excluded from the generated ETag (used for the likes of duplicate send tracking).
        /// </summary>
        [JsonProperty("excludeColumnsFromETag", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection(".NET", Title = "The default list of `Column` names that should be excluded from the generated ETag (used for the likes of duplicate send tracking)")]
        public List<string>? ExcludeColumnsFromETag { get; set; }

        /// <summary>
        /// Get or sets the JSON Serializer to use for JSON property attribution.
        /// </summary>
        [JsonProperty("jsonSerializer", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("CDC", Title = "The JSON Serializer to use for JSON property attribution.", Options = new string[] { "Newtonsoft" },
            Description = "Defaults to `Newtonsoft`.")]
        public string? JsonSerializer { get; set; }

        #endregion

        #region Event

        /// <summary>
        /// Gets or sets the root for the event name by prepending to all event subject names.
        /// </summary>
        [JsonProperty("eventSubjectRoot", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Event", Title = "The root for the event name by prepending to all event subject names via CDC.",
            Description = "Used to enable the sending of messages to the likes of EventHubs, Service Broker, Kafka, etc.", IsImportant = true)]
        public string? EventSubjectRoot { get; set; }

        /// <summary>
        /// Gets or sets the default formatting for the Subject when an Event is published.
        /// </summary>
        [JsonProperty("eventSubjectFormat", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Event", Title = "The default formatting for the Subject when an Event is published via CDC.", Options = new string[] { "NameOnly", "NameAndKey", "NameAndTableKey" },
            Description = "Defaults to `NameOnly`.")]
        public string? EventSubjectFormat { get; set; }

        /// <summary>
        /// Gets or sets the formatting for the Action when an Event is published.
        /// </summary>
        [JsonProperty("eventActionFormat", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Event", Title = "The formatting for the Action when an Event is published via CDC.", Options = new string[] { "None", "PastTense" }, IsImportant = true,
            Description = "Defaults to `None` (no formatting required, i.e. as-is).")]
        public string? EventActionFormat { get; set; }

        /// <summary>
        /// Gets or sets the URI kind for the event source URIs.
        /// </summary>
        [JsonProperty("eventSourceKind", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Event", Title = "The URI kind for the event source URIs for CDC.", Options = new string[] { "None", "Absolute", "Relative", "RelativeOrAbsolute" },
            Description = "Defaults to `Relative` (being a relative path).")]
        public string? EventSourceKind { get; set; }

        /// <summary>
        /// Gets or sets the URI root for the event source by prepending to all event source URIs.
        /// </summary>
        [JsonProperty("eventSourceRoot", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Event", Title = "The URI root for the event source by prepending to all event source URIs for CDC.",
            Description = "The event source is only updated where an `EventSourceKind` is not `None`.")]
        public string? EventSourceRoot { get; set; }

        /// <summary>
        /// Gets or sets the default formatting for the Source when an Event is published.
        /// </summary>
        [JsonProperty("eventSourceFormat", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Event", Title = "The default formatting for the Source when an Event is published via CDC.", Options = new string[] { "NameOnly", "NameAndKey", "NameAndTableKey" },
            Description = "Defaults to `NameAndTableKey` (being the child `Cdc.ModelName` appended with the corresponding table key).")]
        public string? EventSourceFormat { get; set; }

        #endregion

        #region Path

        /// <summary>
        /// Gets or sets the base path (directory) prefix for the artefacts.
        /// </summary>
        [JsonProperty("pathBase", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Path", Title = "The base path (directory) prefix for the Database-related artefacts; other `Path*` properties append to this value when they are not specifically overridden.",
            Description = "Defaults to `AppName` (runtime parameter). For example `Avanade.Application`.")]
        public string? PathBase { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the root Database-related artefacts.
        /// </summary>
        [JsonProperty("pathDatabase", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Path", Title = "The path (directory) for the Schema Database-related artefacts.",
            Description = "Defaults to `PathBase` + `.Database` (literal). For example `Avanade.Application.Database`.")]
        public string? PathDatabase { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the Schema Database-related artefacts.
        /// </summary>
        [JsonProperty("pathDatabaseSchema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Path", Title = "The path (directory) for the Schema Database-related artefacts.",
            Description = "Defaults to `PathDatabase` + `/Schema` (literal). For example `Avanade.Application.Database/Schema`.")]
        public string? PathDatabaseSchema { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the Schema Database-related artefacts.
        /// </summary>
        [JsonProperty("pathDatabaseMigrations", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Path", Title = "The path (directory) for the Schema Database-related artefacts.",
            Description = "Defaults to `PathDatabase` + `/Migrations` (literal). For example `Avanade.Application.Database/Migrations`.")]
        public string? PathDatabaseMigrations { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the CDC-related (.NET) publisher artefacts.
        /// </summary>
        [JsonProperty("PathDotNetPublisher", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Path", Title = "The path (directory) for the CDC-related (.NET) artefacts.",
            Description = "Defaults to `PathBase` + `.Publisher` (literal). For example `Avanade.Application.Publisher`.")]
        public string? PathDotNetPublisher { get; set; }

        #endregion

        #region Namespace

        /// <summary>
        /// Gets or sets the base Namespace (root) for the .NET artefacts.
        /// </summary>
        [JsonProperty("namespaceBase", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Namespace", Title = "The base Namespace (root) for the .NET artefacts.",
            Description = "Defaults to `AppName` (runtime parameter). For example `Avanade.Application`.")]
        public string? NamespaceBase { get; set; }

        /// <summary>
        /// Gets or sets the Namespace (root) for the CDC-related publisher .NET artefacts.
        /// </summary>
        [JsonProperty("NamespaceCdcPublisher", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Namespace", Title = "The Namespace (root) for the CDC-related publisher .NET artefacts.",
            Description = "Defaults to `NamespaceBase` + `.CdcPublisher` (literal). For example `Avanade.Application.CdcPublisher`.")]
        public string? NamespacePublisher { get; set; }

        #endregion

        #region Collections

        /// <summary>
        /// Gets or sets the corresponding <see cref="TableConfig"/> collection.
        /// </summary>
        [JsonProperty("tables", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `Table` collection.", IsImportant = true,
            Markdown = "A `Table` object provides the primary database table configuration for Change Data Capture (CDC), including multiple child table joins to form a composite entity.")]
        public List<TableConfig>? Tables { get; set; }

        #endregion

        #region Non-Config

        /// <summary>
        /// Gets or sets the list of tables that exist within the database.
        /// </summary>
        public List<DbTable>? DbTables { get; private set; }

        /// <summary>
        /// Gets the application name from the <see cref="ICodeGeneratorArgs.Parameters"/>.
        /// </summary>
        public string? AppName => CodeGenArgs!.GetAppName(true);

        /// <summary>
        /// Gets the .NET <see cref="Type"/> that corresponds to the <see cref="IdentifierMappingType"/>.
        /// </summary>
        public string IdentifierMappingDotNetType => IdentifierMappingType switch
        {
            "Int" => "int",
            "Long" => "long",
            "Guid" => "Guid",
            _ => "string"
        };

        /// <summary>
        /// Gets the SQL Type that corresponds to the <see cref="IdentifierMappingType"/>.
        /// </summary>
        public string IdentifierMappingSqlType => IdentifierMappingType switch
        {
            "Int" => "INT",
            "Long" => "BIGINT",
            "Guid" => "UNIQUEIDENTIFIER",
            _ => "NVARCHAR(128)"
        };

        /// <summary>
        /// Gets the list of tables that are to be CDC-enabled.
        /// </summary>
        public List<CdcEnableConfig> CdcEnabledTables = new List<CdcEnableConfig>();

        #endregion

        /// <inheritdoc/>
        protected override void Prepare()
        {
            LoadDbTablesConfig();

            Schema = DefaultWhereNull(Schema, () => "dbo");
            CdcSchema = DefaultWhereNull(CdcSchema, () => "NTangle");
            CdcEnable = DefaultWhereNull(CdcEnable, () => false);
            VersionTrackingTable = DefaultWhereNull(VersionTrackingTable, () => "VersionTracking");
            IdentifierMappingType = DefaultWhereNull(IdentifierMappingType, () => "String");
            IdentifierMappingTable = DefaultWhereNull(IdentifierMappingTable, () => "IdentifierMapping");
            IdentifierMappingStoredProcedure = DefaultWhereNull(IdentifierMappingStoredProcedure, () => "spIdentifierMappingCreate");
            AutoDotNetRename = DefaultWhereNull(AutoDotNetRename, () => "SnakeKebabToPascalCase");
            JsonSerializer = DefaultWhereNull(JsonSerializer, () => "Newtonsoft");
            EventSourceKind = DefaultWhereNull(EventSubjectFormat, () => "Relative");
            EventSourceFormat = DefaultWhereNull(EventSubjectFormat, () => "NameOnly");
            EventSubjectFormat = DefaultWhereNull(EventSubjectFormat, () => "NameAndTableKey");
            EventActionFormat = DefaultWhereNull(EventActionFormat, () => "PastTense");
            PathBase = DefaultWhereNull(PathBase, () => AppName);
            PathDatabase = DefaultWhereNull(PathDatabase, () => $"{PathBase}.Database");
            PathDatabaseSchema = DefaultWhereNull(PathDatabaseSchema, () => $"{PathDatabase}/Schema");
            PathDatabaseMigrations = DefaultWhereNull(PathDatabaseMigrations, () => $"{PathDatabase}/Migrations");
            PathDotNetPublisher = DefaultWhereNull(PathDotNetPublisher, () => $"{PathBase}.Publisher");
            NamespaceBase = DefaultWhereNull(NamespaceBase, () => AppName);
            NamespacePublisher = DefaultWhereNull(NamespacePublisher, () => $"{NamespaceBase}.Publisher");

            Tables = PrepareCollection(Tables);
        }

        /// <summary>
        /// Load the database table and columns configuration.
        /// </summary>
        private void LoadDbTablesConfig()
        {
            CodeGenArgs?.Logger?.Log(LogLevel.Information, $"  Querying database to infer table(s)/column(s) schema...");

            var cs = CodeGenArgs?.ConnectionString ?? throw new CodeGenException("Connection string must be specified via an environment variable or as a command-line option.");

            var sw = Stopwatch.StartNew();
            using var db = new SqlConnection(cs);
            DbTables = OnRamp.Database.Database.GetSchemaAsync(db, false).GetAwaiter().GetResult();

            sw.Stop();
            CodeGenArgs?.Logger?.Log(LogLevel.Information, $"    Database query complete [{sw.ElapsedMilliseconds}ms]");
            CodeGenArgs?.Logger?.Log(LogLevel.Information, string.Empty);
        }

        /// <summary>
        /// Renames for usage in .NET using the <see cref="AutoDotNetRename"/> option.
        /// </summary>
        /// <param name="name">The value to rename.</param>
        /// <returns>The renamed value.</returns>
        public string? RenameForDotNet(string? name)
        {
            if (string.IsNullOrEmpty(name) || AutoDotNetRename == "None")
                return name;

            if (AutoDotNetRename == "PascalCase")
                return StringConverter.ToPascalCase(name);

            // That only leaves SnakeKebabToPascalCase.
            var sb = new StringBuilder();
            foreach (var part in name.Split(new char[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries))
            {
                sb.Append(StringConverter.ToPascalCase(part));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Add Cdc-enabled table.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="table">The table.</param>
        public void AddCdcEnabled(string schema, string table)
        {
            // Must be turned on at the root as a minimum.
            if (IsFalse(CdcEnable))
                return;

            if (CdcEnabledTables.Any(x => x.Schema == schema && x.Table == table))
                return;

            CdcEnabledTables.Add(new CdcEnableConfig { Schema = schema, Table = table });
        }
    }
}