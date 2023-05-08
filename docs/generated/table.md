# 'Table' object (database-driven)

The `Table` object enables the definition of the primary table, one-or-more child tables and their respective join relationships, to enable Change Data Capture (CDC) event publishing. The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude selected columns; with the `AliasColumns` providing a means to rename where required.

<br/>

## Property categories
The `Table` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories.

Category | Description
-|-
[`Key`](#Key) | Provides the _key_ configuration.
[`Columns`](#Columns) | Provides the _Columns_ configuration.
[`Database`](#Database) | Provides the _database_ configuration.
[`.NET`](#NET) | Provides the _.NET_ configuration.
[`Event`](#Event) | Provides the _event_ configuration.
[`Infer`](#Infer) | Provides the _special Column Name inference_ configuration.
[`IdentifierMapping`](#IdentifierMapping) | Provides the _identifier mapping_ configuration.
[`Collections`](#Collections) | Provides related child (hierarchical) configuration.

The properties with a bold name are those that are more typically used (considered more important).

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`name`** | The name of the primary table. [Mandatory]
`schema` | The default schema name used where not otherwise explicitly specified.<br/>&dagger; Defaults to `Root.Schema`.
`table` | The name of the primary table.<br/>&dagger; Defaults to `Name`. This is used to specify the actual underlying database table name (required where the `Name` has been changed to enable uniqueness).
`alias` | The table alias name (must be unique).<br/>&dagger; Will automatically default where not specified; for example a table named `Person` will default to `p`.

<br/>

## Columns
Provides the _Columns_ configuration.

Property | Description
-|-
**`includeColumns`** | The list of `Column` names to be included in the underlying generated output.<br/>&dagger; Where not specified this indicates that all `Columns` are to be included.
**`excludeColumns`** | The list of `Column` names to be excluded from the underlying generated output.<br/>&dagger; Where not specified this indicates no `Columns` are to be excluded.
**`aliasColumns`** | The list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column aliasing/renaming.<br/>&dagger; Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`.

<br/>

## Database
Provides the _database_ configuration.

Property | Description
-|-
`executeStoredProcedure` | The `CDC` _execute_ batch stored procedure name.<br/>&dagger; Defaults to `sp` (literal) + `Name` + `BatchExecute` (literal); e.g. `spNameBatchExecute`.
`completeStoredProcedure` | The `CDC` _complete_ batch stored procedure name.<br/>&dagger; Defaults to `sp` (literal) + `Name` + `BatchComplete` (literal); e.g. `spNameBatchComplete`.
`cdcSchema` | The schema name for the generated `CDC`-related database artefacts.<br/>&dagger; Defaults to `Root.CdcSchema`.
`batchTrackingTable` | The corresponding `CDC` Batch tracking table name.<br/>&dagger; Defaults to `Name` + `BatchTracking` (literal).
`cdcEnable` | Indicates whether to enable `Cdc` within the database for the tables that participate.<br/>&dagger; Defaults to `false`. This option can be overridden for each underlying table referenced.

<br/>

## .NET
Provides the _.NET_ configuration.

Property | Description
-|-
`model` | The .NET model name.<br/>&dagger; Defaults to `Name`.
`orchestratorCtorParams` | The list of additional (non-default) Dependency Injection (DI) parameters for the generated CDC `Orchestrator` constructor.<br/>&dagger; Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred.
`database` | The .NET database `IDatabase` Type name used in the constructor for Dependency Injection (DI).<br/>&dagger; Defaults to `IDatabase`.
`includeColumnsOnDelete` | The list of `Column` names that should be included (in addition to the primary key) for a logical delete.<br/>&dagger; Where a column is not specified in this list its corresponding .NET property will be automatically cleared by the `CdcDataOrchestrator` as the data is technically considered as non-existing.
`excludeColumnsFromETag` | The list of `Column` names that should be excluded from the generated ETag (used for the likes of duplicate send tracking).<br/>&dagger; Defaults to `Root.CdcExcludeColumnsFromETag`.
`tenantIdColumns` | The list of `Column` names that represent the tenant identifier.
`partitionKey` | The partition key.<br/>&dagger; A partition key can be specified using either `PartitionKey` or `PartitionKeyColumns`.
`partitionKeyColumns` | The list of `Column` names that represent the partition key.<br/>&dagger; A partition key can be specified using either `PartitionKey` or `PartitionKeyColumns`.

<br/>

## Event
Provides the _event_ configuration.

Property | Description
-|-
`eventSource` | The Event Source.<br/>&dagger; Defaults to `Schema` + `/` (literal) + `Name` (as lowercase). Note: when used in code-generation the `Root.EventSourceRoot` will be prepended where specified.
`eventSourceFormat` | The default formatting for the Source when an Event is published. Valid options are: `NameOnly`, `NameAndKey`, `NameAndTableKey`.<br/>&dagger; Defaults to `Root.EventSourceFormat`.
`eventSubject` | The Event Subject.<br/>&dagger; Defaults to `ModelName`. Note: when used in code-generation the `Root.EventSubjectRoot` will be prepended where specified.
`eventSubjectFormat` | The default formatting for the Subject when an Event is published. Valid options are: `NameOnly`, `NameAndKey`, `NameAndTableKey`.<br/>&dagger; Defaults to `Root.EventSubjectFormat`.
`eventType` | The Event Type.<br/>&dagger; Defaults to `ModelName`. Note: when used in code-generation the `Root.EventTypeRoot` will be prepended where specified.

<br/>

## Infer
Provides the _special Column Name inference_ configuration.

Property | Description
-|-
`isDeletedColumn` | The column name for the `IsDeleted` (logical delete) capability (if any).<br/>&dagger; Defaults to `Root.ColumnIsDeleted`.

<br/>

## IdentifierMapping
Provides the _identifier mapping_ configuration.

Property | Description
-|-
**`identifierMapping`** | Indicates whether to perform Identifier Mapping (mapping to `GlobalId`) for the primary key.<br/>&dagger; This indicates whether to create a new `GlobalId` property on the _entity_ to house the global mapping identifier to be the reference outside of the specific database realm as a replacement to the existing primary key column(s). Defaults to `Root.IdentifierMapping`.

<br/>

## Collections
Provides related child (hierarchical) configuration.

Property | Description
-|-
**`joins`** | The corresponding [`Join`](join.md) collection.<br/>&dagger; A `Join` object provides the configuration for a table join.
**`where`** | The corresponding [`Where`](where.md) collection.<br/>&dagger; A `Where` object provides the configuration for a table where clause.
`mappings` | The corresponding [`TableMapping`](tablemapping.md) collection.

