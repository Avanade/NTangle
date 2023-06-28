# 'Join' object (database-driven)

The `Join` object defines a join to another (or same) table within the logical CDC entity.  The `IncludeColumns` and `ExcludeColumns` provide a shorthand to include or exclude selected columns; with the `AliasColumns` providing a means to rename where required.

<br/>

## Property categories
The `Join` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories.

Category | Description
-|-
[`Key`](#Key) | Provides the _key_ configuration.
[`JoinTo`](#JoinTo) | Provides the _join to_ configuration.
[`Database`](#Database) | Provides the _database_ configuration.
[`Columns`](#Columns) | Provides the _Columns_ configuration.
[`.NET`](#NET) | Provides the _.NET_ configuration.
[`IdentifierMapping`](#IdentifierMapping) | Provides the _identifier mapping_ configuration.
[`Collections`](#Collections) | Provides related child (hierarchical) configuration.

The properties with a bold name are those that are more typically used (considered more important).

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`name`** | The unique name. [Mandatory]<br/>&dagger; A unique name is required where the same `Table` is referenced more than once within a logical CDC entity. However, generally, this will represent the unique name of the table within the database.
`schema` | The schema name of the table to join.<br/>&dagger; Defaults to `Cdc.Schema`; i.e. same schema.
`table` | The name of the table to join.<br/>&dagger; Defaults to `Name`. This is used to specify the actual underlying database table name (required where the `Name` has been changed to enable uniqueness).
`alias` | The `Schema` and `Table` alias name.<br/>&dagger; Will automatically default where not specified.
**`type`** | The SQL join type. Valid options are: `Cdc`, `Inner`, `Left`, `Right`, `Full`.<br/>&dagger; Defaults to `Cdc`. The `Cdc` value indicates this is a related secondary table (within a logical CDC entity) that also has Change Data Capture turned on and equally needs to be monitored for changes.

<br/>

## JoinTo
Provides the _join to_ configuration.

Property | Description
-|-
**`joinTo`** | The name of the table to join to (must be previously specified).<br/>&dagger; Defaults to parent `Table.Name`.
**`joinToSchema`** | The schema name of the table to join to.<br/>&dagger; Defaults to parent `Table.Schema`.
`joinCardinality` | The join cardinality being whether there is a One-to-Many or One-to-One relationship. Valid options are: `OneToMany`, `OneToOne`.<br/>&dagger; Defaults to `OneToMany`. This represents the Parent (`JoinTo`) to child (_this_) relationship.

<br/>

## Database
Provides the _database_ configuration.

Property | Description
-|-
`cdcEnable` | Indicates whether to enable `Cdc` within the database for the tables that participate.<br/>&dagger; Defaults to `false`. This option can be overridden for each underlying table referenced.
`querySizeMultiplier` | The query size multiplier for the CDC-Join.<br/>&dagger; Defaults to `1.0`. This is applied to the execute stored procedure `@MaxQuerySize` parameter to allow tailoring on the join query (`TOP`) sizes to optimize selection. Must be greater than zero and less than or equal to 100.

<br/>

## Columns
Provides the _Columns_ configuration.

Property | Description
-|-
**`includeColumns`** | The list of `Column` names to be included in the underlying generated output.<br/>&dagger; Where not specified this indicates that all `Columns` are to be included.
**`excludeColumns`** | The list of `Column` names to be excluded from the underlying generated output.<br/>&dagger; Where not specified this indicates no `Columns` are to be excluded.
**`aliasColumns`** | The list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column renaming.<br/>&dagger; Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`

<br/>

## .NET
Provides the _.NET_ configuration.

Property | Description
-|-
`model` | The .NET model name.<br/>&dagger; Defaults to `Name`.
`property` | The .NET property name.<br/>&dagger; Defaults to `Model` where `JoinCardinality` is `OneToOne`; otherwise, it will be the `Model` pluralized.
`includeColumnsOnDelete` | The list of `Column` names that should be included (in addition to the primary key) for a logical delete.<br/>&dagger; Where a column is not specified in this list its corresponding .NET property will be automatically cleared by the `CdcDataOrchestrator` as the data is technically considered as non-existing.
`excludeColumnsFromETag` | The list of `Column` names that should be excluded from the generated ETag (used for the likes of duplicate send tracking).<br/>&dagger; Defaults to `CodeGeneration.CdcExcludeColumnsFromETag`.

<br/>

## IdentifierMapping
Provides the _identifier mapping_ configuration.

Property | Description
-|-
**`identifierMapping`** | Indicates whether to perform Identifier Mapping (mapping to `GlobalId`) for the primary key.<br/>&dagger; This indicates whether to create a new `GlobalId` property on the _entity_ to house the global mapping identifier to be the reference outside of the specific database realm as a replacement to the existing primary key column(s).

<br/>

## Collections
Provides related child (hierarchical) configuration.

Property | Description
-|-
`on` | The corresponding [`JoinOn`](joinon.md) collection.
`mappings` | The corresponding [`JoinMapping`](joinmapping.md) collection.

