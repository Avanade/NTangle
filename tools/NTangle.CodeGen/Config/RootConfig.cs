// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using DbEx;
using DbEx.DbSchema;
using DbEx.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using OnRamp;
using OnRamp.Config;
using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NTangle.CodeGen.Config
{
    /// <summary>
    /// Represents the global database code-generation configuration.
    /// </summary>
    [CodeGenClass("Root", Title = "The `Root` object (database-driven)",
        Description = "The `RootConfig` object defines the global properties that are used to drive the underlying database-driven CDC-oriented code-generation.",
        ExampleMarkdown = @"A YAML configuration example is as follows:
``` yaml
```")]
    [CodeGenCategory("Key", Title = "Provides the _key_ configuration.")]
    [CodeGenCategory("Identifier", Title = "Provides the _Identifier_ configuration.")]
    [CodeGenCategory("Infer", Title = "Provides the _special Column Name inference_ configuration.")]
    [CodeGenCategory(".NET", Title = "Provides the _.NET_ configuration.")]
    [CodeGenCategory("Event", Title = "Provides the _event_ configuration.")]
    [CodeGenCategory("Outbox", Title = "Provides the _outbox_ configuration.")]
    [CodeGenCategory("Path", Title = "Provides the _Path (Directory)_ configuration for the generated artefacts.")]
    [CodeGenCategory("Namespace", Title = "Provides the _.NET Namespace_ configuration for the generated artefacts.")]
    [CodeGenCategory("Collections", Title = "Provides related child (hierarchical) configuration.")]
    public sealed class RootConfig : ConfigRootBase<RootConfig>
    {
        private static readonly char[] _separators = ['_', '-'];

        #region Key

        /// <summary>
        /// Gets or sets the default `Schema` name of the where the existing tables are defined in the database.
        /// </summary>
        [JsonPropertyName("schema")]
        [CodeGenProperty("Key", Title = "The default `Schema` name where the existing tables are defined within the database.", IsImportant = true,
            Description = "This is used as the default `Schema` for all child objects. Defaults to `dbo` (literal).")]
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the schema name for the `Cdc`-related database artefacts.
        /// </summary>
        [JsonPropertyName("cdcSchema")]
        [CodeGenProperty("Key", Title = "The schema name for the _ntangle_ generated `CDC`-related database artefacts.",
            Description = "Defaults to `NTangle` (literal).")]
        public string? CdcSchema { get; set; }

        /// <summary>
        /// Indicates whether to create the `Cdc`-Schema within the database.
        /// </summary>
        [JsonPropertyName("cdcSchemaCreate")]
        [CodeGenProperty("Key", Title = "Indicates whether to create the `CdcSchema` within the database.",
            Description = "Defaults to `false`.")]
        public bool? CdcSchemaCreate { get; set; }

        /// <summary>
        /// Gets or sets the table name for the `Cdc`-VersionTracking.
        /// </summary>
        [JsonPropertyName("versionTrackingTable")]
        [CodeGenProperty("Key", Title = "The table name for the `Cdc`-VersionTracking.",
            Description = "Defaults to `VersionTracking` (literal).")]
        public string? VersionTrackingTable { get; set; }

        /// <summary>
        /// Indicates whether to enable `Cdc` within the database for the tables that participate.
        /// </summary>
        [JsonPropertyName("cdcEnable")]
        [CodeGenProperty("Key", Title = "Indicates whether to enable `Cdc` within the database for the tables that participate.",
            Description = "Defaults to `false`. This option can be overridden for each underlying table referenced.")]
        public bool? CdcEnable { get; set; }

        #endregion

        #region Identifier

        /// <summary>
        /// Indicates whether to include the generation of the generic `CDC`-IdentifierMapping database capabilities.
        /// </summary>
        [JsonPropertyName("identifierMapping")]
        [CodeGenProperty("Identifier", Title = "Indicates whether to include the generation of the generic `CDC`-IdentifierMapping database capabilities.",
            Description = "Where set to `true` each underlying `Table` and corresponding `Join` must set `IdentifierMapping` explicitly.")]
        public bool? IdentifierMapping { get; set; }

        /// <summary>
        /// Gets or sets the identifier mapping <see cref="Type"/>.
        /// </summary>
        [JsonPropertyName("identifierMappingType")]
        [CodeGenProperty("Identifier", Title = "The type for the identifier mapping value.", Options = ["String", "Int", "Long", "Guid"],
            Description = "Defaults to `String`.")]
        public string? IdentifierMappingType { get; set; }

        /// <summary>
        /// Gets or sets the table name for the `Cdc`-IdentifierMapping.
        /// </summary>
        [JsonPropertyName("identifierMappingTable")]
        [CodeGenProperty("Identifier", Title = "The table name for the `Cdc`-IdentifierMapping.",
            Description = "Defaults to `IdentifierMapping` (literal).")]
        public string? IdentifierMappingTable { get; set; }

        /// <summary>
        /// Gets or sets the stored procedure name for the `Cdc`-IdentifierMapping create.
        /// </summary>
        [JsonPropertyName("identifierMappingStoredProcedure")]
        [CodeGenProperty("Identifier", Title = "The stored procedure name for the `Cdc`-IdentifierMapping create.",
            Description = "Defaults to `spIdentifierMappingCreate` (literal).")]
        public string? IdentifierMappingStoredProcedure { get; set; }

        #endregion

        #region Infer

        /// <summary>
        /// Gets or sets the column name for the `IsDeleted` capability.
        /// </summary>
        [JsonPropertyName("isDeletedColumn")]
        [CodeGenProperty("Infer", Title = "The column name for the `IsDeleted` (logical delete) capability (if any).",
            Description = "Defaults to `IsDeleted`.")]
        public string? IsDeletedColumn { get; set; }

        #endregion

        #region .NET

        /// <summary>
        /// Gets or sets the option to automatically rename the SQL Tables and Columns for use in .NET.
        /// </summary>
        [JsonPropertyName("autoDotNetRename")]
        [CodeGenProperty(".NET", Title = "The option to automatically rename the SQL Tables and Columns for use in .NET.", Options = ["None", "PascalCase", "SnakeKebabToPascalCase"],
            Description = "Defaults to `SnakeKebabToPascalCase` which will remove any underscores or hyphens separating each word and capitalize the first character of each; e.g. `internal-customer_id` would be renamed as `InternalCustomerId`. The `PascalCase` option will capatilize the first character only.")]
        public string? AutoDotNetRename { get; set; }

        /// <summary>
        /// Gets or sets the default list of `Column` names that should be excluded from the generated ETag (used for the likes of duplicate send tracking).
        /// </summary>
        [JsonPropertyName("excludeColumnsFromETag")]
        [CodeGenPropertyCollection(".NET", Title = "The default list of `Column` names that should be excluded from the generated ETag (used for the likes of duplicate send tracking)")]
        public List<string>? ExcludeColumnsFromETag { get; set; }

        /// <summary>
        /// Get or sets the JSON Serializer to use for JSON property attribution.
        /// </summary>
        [JsonPropertyName("jsonSerializer")]
        [CodeGenProperty("CDC", Title = "The JSON Serializer to use for JSON property attribution.", Options = ["SystemText", "Newtonsoft"],
            Description = "Defaults to `SystemText`.")]
        public string? JsonSerializer { get; set; }

        /// <summary>
        /// Gets or sets the type of service that manages the underlying orchestrator.
        /// </summary>
        [JsonPropertyName("service")]
        [CodeGenProperty("CDC", Title = "The type of service that manages the underlying orchestrator.", Options = ["None", "HostedService", "Service"],
            Description = "Defaults to `None`. A `HostedService` is an `IHostedService` implementation enabling long-running execution; whereas, `Service` is intended for self-managed execution.")]
        public string? Service { get; set; }

        #endregion

        #region Event

        /// <summary>
        /// Gets or sets the root for the event name by prepending to all event subject names.
        /// </summary>
        [JsonPropertyName("eventSubjectRoot")]
        [CodeGenProperty("Event", Title = "The root for the event name by prepending to all event subject names via CDC.",
            Description = "Used to enable the sending of messages to the likes of EventHubs, Service Broker, Kafka, etc.", IsImportant = true)]
        public string? EventSubjectRoot { get; set; }

        /// <summary>
        /// Gets or sets the default formatting for the Subject when an Event is published.
        /// </summary>
        [JsonPropertyName("eventSubjectFormat")]
        [CodeGenProperty("Event", Title = "The default formatting for the Subject when an Event is published via CDC.", Options = ["NameOnly", "NameAndKey", "NameAndTableKey"],
            Description = "Defaults to `NameOnly`.")]
        public string? EventSubjectFormat { get; set; }

        /// <summary>
        /// Gets or sets the formatting for the Action when an Event is published.
        /// </summary>
        [JsonPropertyName("eventActionFormat")]
        [CodeGenProperty("Event", Title = "The formatting for the Action when an Event is published via CDC.", Options = ["None", "PastTense"], IsImportant = true,
            Description = "Defaults to `None` (no formatting required, i.e. as-is).")]
        public string? EventActionFormat { get; set; }

        /// <summary>
        /// Gets or sets the URI kind for the event source URIs.
        /// </summary>
        [JsonPropertyName("eventSourceKind")]
        [CodeGenProperty("Event", Title = "The URI kind for the event source URIs for CDC.", Options = ["None", "Absolute", "Relative", "RelativeOrAbsolute"],
            Description = "Defaults to `Relative` (being a relative path).")]
        public string? EventSourceKind { get; set; }

        /// <summary>
        /// Gets or sets the URI root for the event source by prepending to all event source URIs.
        /// </summary>
        [JsonPropertyName("eventSourceRoot")]
        [CodeGenProperty("Event", Title = "The URI root for the event source by prepending to all event source URIs for CDC.",
            Description = "The event source is only updated where an `EventSourceKind` is not `None`.")]
        public string? EventSourceRoot { get; set; }

        /// <summary>
        /// Gets or sets the default formatting for the Source when an Event is published.
        /// </summary>
        [JsonPropertyName("eventSourceFormat")]
        [CodeGenProperty("Event", Title = "The default formatting for the Source when an Event is published via CDC.", Options = ["NameOnly", "NameAndKey", "NameAndTableKey"],
            Description = "Defaults to `NameAndTableKey` (being the child `Cdc.ModelName` appended with the corresponding table key).")]
        public string? EventSourceFormat { get; set; }

        /// <summary>
        /// Gets or sets the root for the event type by prepending to all event type names.
        /// </summary>
        [JsonPropertyName("eventTypeRoot")]
        [CodeGenProperty("Event", Title = "The root for the event type by prepending to all event type names via CDC.",
            Description = "Used to enable the sending of messages to the likes of EventHubs, Service Broker, Kafka, etc. Defaults to `EventSubjectRoot`.", IsImportant = true)]
        public string? EventTypeRoot { get; set; }

        #endregion

        #region Outbox

        /// <summary>
        /// Indicates whether to generate the event outbox SQL and .NET artefacts.
        /// </summary>
        [JsonPropertyName("outbox")]
        [CodeGenProperty("Outbox", Title = "Indicates whether to generate the event outbox SQL and .NET artefacts.",
            Description = "Defaults to `false`.")]
        public bool? Outbox { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the event outbox table.
        /// </summary>
        [JsonPropertyName("outboxSchema")]
        [CodeGenProperty("Outbox", Title = "The schema name of the event outbox table.",
            Description = "Defaults to `Outbox` (literal).")]
        public string? OutboxSchema { get; set; }

        /// <summary>
        /// Indicates whether to create the `Outbox`-Schema within the database.
        /// </summary>
        [JsonPropertyName("outboxSchemaCreate")]
        [CodeGenProperty("Key", Title = "Indicates whether to create the `OutboxSchema` within the database.",
            Description = "Defaults to `false`.")]
        public bool? OutboxSchemaCreate { get; set; }

        /// <summary>
        /// Gets or sets the name of the event outbox table.
        /// </summary>
        [JsonPropertyName("outboxTable")]
        [CodeGenProperty("Outbox", Title = "The name of the event outbox table.",
            Description = "Defaults to `EventOutbox` (literal).")]
        public string? OutboxTable { get; set; }

        /// <summary>
        /// Gets or sets the stored procedure name for the event outbox enqueue.
        /// </summary>
        [JsonPropertyName("outboxEnqueueStoredProcedure")]
        [CodeGenProperty("Outbox", Title = "The stored procedure name for the event outbox enqueue.",
            Description = "Defaults to `spEventOutboxEnqueue` (literal).")]
        public string? OutboxEnqueueStoredProcedure { get; set; }

        /// <summary>
        /// Gets or sets the stored procedure name for the event outbox dequeue.
        /// </summary>
        [JsonPropertyName("outboxDequeueStoredProcedure")]
        [CodeGenProperty("Outbox", Title = "The stored procedure name for the event outbox dequeue.",
            Description = "Defaults to `spEventOutboxDequeue` (literal).")]
        public string? OutboxDequeueStoredProcedure { get; set; }

        #endregion

        #region Path

        /// <summary>
        /// Gets or sets the base path (directory) prefix for the artefacts.
        /// </summary>
        [JsonPropertyName("pathBase")]
        [CodeGenProperty("Path", Title = "The base path (directory) prefix for the Database-related artefacts; other `Path*` properties append to this value when they are not specifically overridden.",
            Description = "Defaults to `AppName` (runtime parameter). For example `Avanade.Application`.")]
        public string? PathBase { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the root Database-related artefacts.
        /// </summary>
        [JsonPropertyName("pathDatabase")]
        [CodeGenProperty("Path", Title = "The path (directory) for the Schema Database-related artefacts.",
            Description = "Defaults to `PathBase` + `.Database` (literal). For example `Avanade.Application.Database`.")]
        public string? PathDatabase { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the Schema Database-related artefacts.
        /// </summary>
        [JsonPropertyName("pathDatabaseSchema")]
        [CodeGenProperty("Path", Title = "The path (directory) for the Schema Database-related artefacts.",
            Description = "Defaults to `PathDatabase` + `/Schema` (literal). For example `Avanade.Application.Database/Schema`.")]
        public string? PathDatabaseSchema { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the Schema Database-related artefacts.
        /// </summary>
        [JsonPropertyName("pathDatabaseMigrations")]
        [CodeGenProperty("Path", Title = "The path (directory) for the Schema Database-related artefacts.",
            Description = "Defaults to `PathDatabase` + `/Migrations` (literal). For example `Avanade.Application.Database/Migrations`.")]
        public string? PathDatabaseMigrations { get; set; }

        /// <summary>
        /// Gets or sets the path (directory) for the CDC-related (.NET) publisher artefacts.
        /// </summary>
        [JsonPropertyName("pathDotNetPublisher")]
        [CodeGenProperty("Path", Title = "The path (directory) for the CDC-related (.NET) artefacts.",
            Description = "Defaults to `PathBase` + `.Publisher` (literal). For example `Avanade.Application.Publisher`.")]
        public string? PathDotNetPublisher { get; set; }

        #endregion

        #region Namespace

        /// <summary>
        /// Gets or sets the base Namespace (root) for the .NET artefacts.
        /// </summary>
        [JsonPropertyName("namespaceBase")]
        [CodeGenProperty("Namespace", Title = "The base Namespace (root) for the .NET artefacts.",
            Description = "Defaults to `AppName` (runtime parameter). For example `Avanade.Application`.")]
        public string? NamespaceBase { get; set; }

        /// <summary>
        /// Gets or sets the Namespace (root) for the CDC-related publisher .NET artefacts.
        /// </summary>
        [JsonPropertyName("namespacePublisher")]
        [CodeGenProperty("Namespace", Title = "The Namespace (root) for the CDC-related Publisher .NET artefacts.",
            Description = "Defaults to `NamespaceBase` + `.Publisher` (literal). For example `Avanade.Application.Publisher`.")]
        public string? NamespacePublisher { get; set; }

        /// <summary>
        /// Gets or sets the Namespace (root) for the outbox-related .NET artefacts.
        /// </summary>
        [JsonPropertyName("namespaceOutbox")]
        [CodeGenProperty("Namespace", Title = "The Namespace (root) for the Outbox-related Publisher .NET artefacts.",
            Description = "Defaults to `NamespacePublisher`.")]
        public string? NamespaceOutbox { get; set; }

        #endregion

        #region Collections

        /// <summary>
        /// Gets or sets the corresponding <see cref="TableConfig"/> collection.
        /// </summary>
        [JsonPropertyName("tables")]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `Table` collection.", IsImportant = true,
            Description = "A `Table` object provides the primary database table configuration for Change Data Capture (CDC), including multiple child table joins to form a composite entity.")]
        public List<TableConfig>? Tables { get; set; }

        #endregion

        #region Non-Config

        /// <summary>
        /// Gets or sets the list of tables that exist within the database.
        /// </summary>
        public List<DbTableSchema>? DbTables { get; private set; }

        /// <summary>
        /// Gets the application name from the <see cref="ICodeGeneratorArgs.Parameters"/>.
        /// </summary>
        public string? AppName => CodeGenArgs!.GetAppName(true);

        /// <summary>
        /// Indicates whether the scripted template is gen-once only.
        /// </summary>
        public bool? IsGenOnce => (bool?)RuntimeParameters.GetValueOrDefault(nameof(OnRamp.Scripts.CodeGenScriptItem.IsGenOnce));

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
            _ => "NVARCHAR(127)"
        };

        /// <summary>
        /// Gets the list of tables that are to be CDC-enabled.
        /// </summary>
        public List<CdcEnableConfig> CdcEnabledTables { get; } = [];

        /// <summary>
        /// Gets the list of hosted services.
        /// </summary>
        public List<TableConfig> HostedServices => Tables!.Where(x => x.Service == "HostedService").ToList();

        /// <summary>
        /// Gets the list of services.
        /// </summary>
        public List<TableConfig> Services => Tables!.Where(x => x.Service == "Service").ToList();

        /// <summary>
        /// Indicates whether there are any <see cref="Services"/> or <see cref="HostedServices"/>.
        /// </summary>
        public bool HasAnyServices => Services.Count > 0 || HostedServices.Count > 0;

        #endregion

        /// <inheritdoc/>
        protected override async Task PrepareAsync()
        {
            await LoadDbTablesConfigAsync().ConfigureAwait(false);

            Schema = DefaultWhereNull(Schema, () => "dbo");
            CdcSchema = DefaultWhereNull(CdcSchema, () => "NTangle");
            CdcEnable = DefaultWhereNull(CdcEnable, () => false);
            VersionTrackingTable = DefaultWhereNull(VersionTrackingTable, () => "VersionTracking");
            IdentifierMappingType = DefaultWhereNull(IdentifierMappingType, () => "String");
            IdentifierMappingTable = DefaultWhereNull(IdentifierMappingTable, () => "IdentifierMapping");
            IdentifierMappingStoredProcedure = DefaultWhereNull(IdentifierMappingStoredProcedure, () => "spIdentifierMappingCreate");
            IsDeletedColumn = DefaultWhereNull(IsDeletedColumn, () => "IsDeleted");
            AutoDotNetRename = DefaultWhereNull(AutoDotNetRename, () => "SnakeKebabToPascalCase");
            JsonSerializer = DefaultWhereNull(JsonSerializer, () => "SystemText");
            Service = DefaultWhereNull(Service, () => "None");
            EventSourceKind = DefaultWhereNull(EventSubjectFormat, () => "Relative");
            EventSourceFormat = DefaultWhereNull(EventSubjectFormat, () => "NameAndTableKey");
            EventSubjectFormat = DefaultWhereNull(EventSubjectFormat, () => "NameOnly");
            EventActionFormat = DefaultWhereNull(EventActionFormat, () => "PastTense");
            EventTypeRoot = DefaultWhereNull(EventTypeRoot, () => EventSubjectRoot);
            OutboxSchema = DefaultWhereNull(OutboxSchema, () => "Outbox");
            OutboxSchemaCreate = DefaultWhereNull(OutboxSchemaCreate, () => false);
            OutboxTable = DefaultWhereNull(OutboxTable, () => "EventOutbox");
            OutboxEnqueueStoredProcedure = DefaultWhereNull(OutboxEnqueueStoredProcedure, () => $"sp{OutboxTable}Enqueue");
            OutboxDequeueStoredProcedure = DefaultWhereNull(OutboxDequeueStoredProcedure, () => $"sp{OutboxTable}Dequeue");
            PathBase = DefaultWhereNull(PathBase, () => AppName);
            PathDatabase = DefaultWhereNull(PathDatabase, () => $"{PathBase}.Database");
            PathDatabaseSchema = DefaultWhereNull(PathDatabaseSchema, () => $"{PathDatabase}/Schema");
            PathDatabaseMigrations = DefaultWhereNull(PathDatabaseMigrations, () => $"{PathDatabase}/Migrations");
            PathDotNetPublisher = DefaultWhereNull(PathDotNetPublisher, () => $"{PathBase}.Publisher");
            NamespaceBase = DefaultWhereNull(NamespaceBase, () => AppName);
            NamespacePublisher = DefaultWhereNull(NamespacePublisher, () => $"{NamespaceBase}.Publisher");
            NamespaceOutbox = DefaultWhereNull(NamespaceOutbox, () => NamespacePublisher);

            Tables = await PrepareCollectionAsync(Tables).ConfigureAwait(false);
        }

        /// <summary>
        /// Load the database table and columns configuration.
        /// </summary>
        private async Task LoadDbTablesConfigAsync()
        {
            CodeGenArgs!.Logger?.Log(LogLevel.Information, $"  Querying database to infer table(s)/column(s) schema...");

            var cs = CodeGenArgs.ConnectionString ?? throw new CodeGenException("Connection string must be specified via an environment variable or as a command-line option.");

            var sw = Stopwatch.StartNew();
            var db = (CodeGenArgs.GetCreateDatabase(false)?.Invoke(CodeGenArgs.ConnectionString!)) ?? throw new CodeGenException(this, null, "A database provider must be specified during application startup, consider using the likes of 'UseSqlServer' to specify; e.g: 'CodeGenConsole.Create(\"...\").UseSqlServer().RunAsync(args)'.");
            var csb = new SqlConnectionStringBuilder(CodeGenArgs.ConnectionString);
            DbTables = await db.SelectSchemaAsync(new SqlServerSchemaConfig(csb.InitialCatalog)).ConfigureAwait(false);

            sw.Stop();
            CodeGenArgs.Logger?.Log(LogLevel.Information, "{Content}", $"    Database schema query complete [{sw.Elapsed.TotalMilliseconds}ms]");
            CodeGenArgs.Logger?.Log(LogLevel.Information, "{Content}", string.Empty);
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
            foreach (var part in name.Split(_separators, StringSplitOptions.RemoveEmptyEntries))
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