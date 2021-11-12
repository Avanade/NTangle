# The 'Root' object (database-driven)

The `CdcRootConfig` object defines the global properties that are used to drive the underlying database-driven CDC-oriented code-generation.

<br/>

## Property categories
The `Root` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories.

Category | Description
-|-
[`Key`](#Key) | Provides the _key_ configuration.
[`Identifier`](#Identifier) | Provides the _Identifier_ configuration.
[`Infer`](#Infer) | Provides the _special Column Name inference_ configuration.
[`.NET`](#NET) | Provides the _.NET_ configuration.
[`Event`](#Event) | Provides the _event_ configuration.
[`Path`](#Path) | Provides the _Path (Directory)_ configuration for the generated artefacts.
[`Namespace`](#Namespace) | Provides the _.NET Namespace_ configuration for the generated artefacts.
[`Collections`](#Collections) | Provides related child (hierarchical) configuration.

The properties with a bold name are those that are more typically used (considered more important).

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`schema`** | The default `Schema` name where the existing tables are defined within the database.<br/>&dagger; This is used as the default `Schema` for all child objects. Defaults to `dbo` (literal).
`cdcSchema` | The schema name for the _ntangle_ generated `CDC`-related database artefacts.<br/>&dagger; Defaults to `NTangle` (literal).
`cdcSchemaCreate` | Indicates whether to create the CDC Schema within the database.<br/>&dagger; Defaults to `false`.
`versionTrackingTable` | The table name for the `Cdc`-VersionTracking.<br/>&dagger; Defaults to `VersionTracking` (literal).
`cdcEnable` | Indicates whether to enable `Cdc` within the database for the tables that participate.<br/>&dagger; Defaults to `false`. This option can be overridden for each underlying table referenced.

<br/>

## Identifier
Provides the _Identifier_ configuration.

Property | Description
-|-
`identifierMapping` | Indicates whether to include the generation of the generic `CDC`-IdentifierMapping database capabilities.<br/>&dagger; Where set to `true` each underlying `Table` and corresponding `Join` must set `IdentifierMapping` explicitly.
`identifierMappingType` | The type for the identifier mapping value. Valid options are: `String`, `Int`, `Long`, `Guid`.<br/>&dagger; Defaults to `String`.
`identifierMappingTable` | The table name for the `Cdc`-IdentifierMapping.<br/>&dagger; Defaults to `IdentifierMapping` (literal).
`identifierMappingStoredProcedure` | The stored procedure name for the `Cdc`-IdentifierMapping create.<br/>&dagger; Defaults to `spIdentifierMappingCreate` (literal).

<br/>

## Infer
Provides the _special Column Name inference_ configuration.

Property | Description
-|-
`isDeletedColumn` | The column name for the `IsDeleted` (logical delete) capability (if any).<br/>&dagger; Defaults to `IsDeleted`.

<br/>

## .NET
Provides the _.NET_ configuration.

Property | Description
-|-
`autoDotNetRename` | The option to automatically rename the SQL Tables and Columns for use in .NET. Valid options are: `None`, `PascalCase`, `SnakeKebabToPascalCase`.<br/>&dagger; Defaults to `SnakeKebabToPascalCase` which will remove any underscores or hyphens separating each word and capitalize the first character of each; e.g. `internal-customer_id` would be renamed as `InternalCustomerId`. The `PascalCase` option will capatilize the first character only.
`excludeColumnsFromETag` | The default list of `Column` names that should be excluded from the generated ETag (used for the likes of duplicate send tracking)

<br/>

## Event
Provides the _event_ configuration.

Property | Description
-|-
**`eventSubjectRoot`** | The root for the event name by prepending to all event subject names via CDC.<br/>&dagger; Used to enable the sending of messages to the likes of EventHubs, Service Broker, Kafka, etc.
`eventSubjectFormat` | The default formatting for the Subject when an Event is published via CDC. Valid options are: `NameOnly`, `NameAndKey`, `NameAndTableKey`.<br/>&dagger; Defaults to `NameOnly`.
**`eventActionFormat`** | The formatting for the Action when an Event is published via CDC. Valid options are: `None`, `PastTense`.<br/>&dagger; Defaults to `None` (no formatting required, i.e. as-is).
`eventSourceKind` | The URI kind for the event source URIs for CDC. Valid options are: `None`, `Absolute`, `Relative`, `RelativeOrAbsolute`.<br/>&dagger; Defaults to `Relative` (being a relative path).
`eventSourceRoot` | The URI root for the event source by prepending to all event source URIs for CDC.<br/>&dagger; The event source is only updated where an `EventSourceKind` is not `None`.
`eventSourceFormat` | The default formatting for the Source when an Event is published via CDC. Valid options are: `NameOnly`, `NameAndKey`, `NameAndTableKey`.<br/>&dagger; Defaults to `NameAndTableKey` (being the child `Cdc.ModelName` appended with the corresponding table key).

<br/>

## Path
Provides the _Path (Directory)_ configuration for the generated artefacts.

Property | Description
-|-
`pathBase` | The base path (directory) prefix for the Database-related artefacts; other `Path*` properties append to this value when they are not specifically overridden.<br/>&dagger; Defaults to `AppName` (runtime parameter). For example `Avanade.Application`.
`pathDatabase` | The path (directory) for the Schema Database-related artefacts.<br/>&dagger; Defaults to `PathBase` + `.Database` (literal). For example `Avanade.Application.Database`.
`pathDatabaseSchema` | The path (directory) for the Schema Database-related artefacts.<br/>&dagger; Defaults to `PathDatabase` + `/Schema` (literal). For example `Avanade.Application.Database/Schema`.
`pathDatabaseMigrations` | The path (directory) for the Schema Database-related artefacts.<br/>&dagger; Defaults to `PathDatabase` + `/Migrations` (literal). For example `Avanade.Application.Database/Migrations`.
`pathDotNetPublisher` | The path (directory) for the CDC-related (.NET) artefacts.<br/>&dagger; Defaults to `PathBase` + `.Publisher` (literal). For example `Avanade.Application.Publisher`.

<br/>

## Namespace
Provides the _.NET Namespace_ configuration for the generated artefacts.

Property | Description
-|-
`namespaceBase` | The base Namespace (root) for the .NET artefacts.<br/>&dagger; Defaults to `AppName` (runtime parameter). For example `Avanade.Application`.
`namespacePublisher` | The Namespace (root) for the CDC-related publisher .NET artefacts.<br/>&dagger; Defaults to `NamespaceBase` + `.Publisher` (literal). For example `Avanade.Application.Publisher`.

<br/>

## Collections
Provides related child (hierarchical) configuration.

Property | Description
-|-
**`tables`** | The corresponding [`Table`](table.md) collection.<br/>&dagger; A `Table` object provides the primary database table configuration for Change Data Capture (CDC), including multiple child table joins to form a composite entity.

