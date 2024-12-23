# Change log

Represents the **NuGet** versions.

## v3.0.2
- *Fixed:* The completion stage was incorrectly using the `EntityKey` versus `TableKey` (where using global identifiers) for the version hashing resulting in re-publishing where not required; code-generation template corrected and is required for fix.

## V3.0.1
- *Fixed:* Generated `ExecuteBatch.sql` fixed to not invoke `sys.fn_cdc_get_min_lsn` twice but use returned result as intended.

## v3.0.0
- *Enhancement:* Major **"Sidecar"** feature added based on feedback from the community. 
  - The existing implementation required all generated supporting database capabilities to be within the "source" database itself; a new option has been added to generate a separate "sidecar" database to manage. This minimizes the impact on the "source" database.
  - The "source" database will still require the database CDC (change-data-capture) capabilities to be enabled.
  - The new [`EntitySidecarOrchestratorBase`](./src/NTangle/Cdc/EntitySidecarOrchestratorBase.cs) will invoke a single statement to leverage the CDC capabilities and perform the resulting data selection; see the [`ContactExecuteBatch.sql`](./samples/SqlServerSidecarDemo/SqlServerSidecarDemo.Publisher/Resources/Generated/ContactExecuteBatch.sql).
  - The required `NTangle` (and optional `Outbox`) schema(s), table(s) and stored procedures will be generated within the "sidecar" database.
  - Note that there are _no_ cross database dependencies; as such, the "sidecar" database can be hosted separately, be on a difference versions, etc. as required. The .NET orchestrator logic will _require_ access to both databases to function.
  - An additional `ExecuteExplicitAsync` method has been added to enable explicit primary keys to be passed bypassing CDC; useful for one-offs, or where triggering without CDC (i.e. Debezium, etc) to get the best of both worlds.
  - Support now for `net8.0`+ only; older .NET versions will need to use the existing implementation or upgrade.
- Notes:
  - This version contains a number of _breaking_ and generated artefact changes; all existing generated artefacts should re-generated.
  - It is recommended migrating to the new "sidecar" feature where possible; the existing implementation will be deprecated in the future.
	- The `NTangle` solution template (`dotnet new ntangle`) has been updated to default to the new "sidecar" feature.

## v2.6.1
- *Fixed:* As a result of the `v2.6.0` enhancements the generated `IdentifierMappingMapper.cs` and `VersionTrackingMapper.cs` are not required. This fix will ensure these files are no longer generated; as such these should be removed from any existing solution after a re-gen.
- *Fixed:* The base `EntityOrchestrator` classes have also been corrected to remove the `IdentifierMappingMapper` and `VersionTrackingMapper` references.

## v2.6.0
- *Enhancement:* Database code-generation defaults to the use of [JSON](https://learn.microsoft.com/en-us/sql/relational-databases/json/json-data-sql-server)-serialized parameters versus UDT/TVP to minimize the need for additional database objects; specifically [User-Defined Types](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-type-transact-sql) (UDT).
  - This will now require a SQL Server version of 2016 or later; use earlier _DbEx_ versions that use UDT/TVP which are supported on earlier SQL Server versions.
- *Enhancement:* All code-generated stored procedures now use `CREATE OR ALTER`; again, requires SQL Server 2016 or later.

## v2.5.2
- *Fixed:* Updated `CoreEx` (`v3.15.0`) and other dependencies.
- *Fixed:* Simplify event outbox C# code-generation templates for primary constructor usage.

## v2.5.1
- *Fixed:* Updated `CoreEx` (`v3.13.0`) and other dependencies.

## v2.5.0
- *Enhancement:* Updated `DbEx` (`v2.5.0`) and `CoreEx` (`v3.12.0`) and other dependencies.

## v2.4.0
- *Enhancement:* Updated `DbEx` (`v2.4.0`) and `CoreEx` (`v3.9.0`) and other dependencies.
- *Enhancement:* Updated the `dotnet new ntangle` template to target `net8.0` and [isolated](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide) functions.

## v2.3.6
- *Fixed:* Updated `DbEx` (`v2.3.15`) and `CoreEx` (`v3.8.1`) and other dependencies.
- *Fixed*: Updated `OnRamp` (`2.0.0`) which necessitated internal change from `Newtonsoft.Json` (now deprecated) to `System.Text.Json`.

## v2.3.5
- *Fixed:* Updated `DbEx` (`v2.3.13`) and `CoreEx` (`v3.6.1`) and other dependencies.
- *Fixed:* Added `net8.0` support.
- *Fixed:* The `EntityOrchestrator` mapping code generation for sub-classes was not always being generated correctly; non-compiling missing `,` formatting corrected.

## v2.3.4
- *Fixed:* Updated `DbEx` (`v2.3.12`) and `CoreEx` (`v3.4.1`).

## v2.3.3
- *Fixed:* Stored procedure `spXxxBatchReset` updated to correctly set the completed date within the batch tracking table.

## v2.3.2
- *Enhancement:* Added new `spXxxBatchReset` stored procedure to reset the bacth tracking to the latest lsn. Can be used to fix data loss batch by completing with the latest lsn. Also useful to support unit testing.
- *Fixed:* Updated `DbEx` (`v2.3.11`) and `CoreEx` (`v3.3.1`).

## v2.3.1
- *Fixed:* Updated `DbEx` (`v2.3.8`) and `OnRamp` (`v1.0.8`).

## v2.3.0
- *Enhancement:* Separated the code-generation tooling logic out into a separate package `NTangle.CodeGen` as this is not required to be deployed with the underlying runtime logic. This is a **breaking change**, in that previous references will need to be updated to the new package.
- *Enhancement:* The version tracking table has had a timestamp column added.
- *Fixed:* Entity code-generation now correctly references the actual underlying table name versus the alias.

## v2.2.3
- *Fixed:* Joins without `IdentifierMapping` were incorrectly generated with mapping code where the root table has mapping, and the joined table did not. The `GlobalId` JSON name can now also be renamed.

## v2.2.2
- *Fixed:* Updated `CoreEx` (`v3.3.0`) and `DbEx` (`v2.3.6`).

## v2.2.1
- *Fixed:* Issue with `Joins` where the `JoinTo` property within the YAML was validating against an underlying table name versus the unqiue `Table.Name` property (as intended). The `JoinToSchema` property should have been internal only and has not been corrected.

## v2.2.0
- *Enhancement:* Added `publisher` option with values `Console|Function|None`, where `Console` is the default, and `Function` represents [Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview), for the `NTangle.Template` package.
- *Fixed:* The `IdentifierMappingMapper.cs` file was incorrrectly being generated when not required.

## v2.1.0
- *Enhancement:* Added `CdcService<TOrchestrator, TEntity>` that provides the base Change Data Capture (CDC) self-orchestrated service capabilities; used for the likes of Azure Functions.
- *Enhancement:* Added `service: Service` YAML configuration to enable the new  `CdcService<TOrchestrator, TEntity>` code-generation. The `IServiceCollectionExtensions` code-generation also updated to include.
- *Fixed:* Primary table `excludeColumns` did not previously allow exclusion of columns used in join or key.
- *Fixed:* Where `name` and `table` were different the correct value was not always being used for the YAML validation.

## v2.0.2
- *Enhancement:* Updated `CoreEx` (`v3.0.0`) and `DbEx` (`v2.3.5`).
- *Enhancement:* Added `net6.0` and `net7.0` support in addition to [.NET Standard](https://learn.microsoft.com/en-us/dotnet/standard/net-standard#when-to-target-net50-or-net60-vs-netstandard) to the `NTangle` package. This will allow access to additional features per version where required, and overall performance improvements.
- *Enhancement:* Included C# code-generation templates updated; target `net6.0`+ only.

## v2.0.1
- *Fixed:* `TableConfig.BatchTrackingTable` JSON serialization name corrected.
- *Fixed:* Commented Azure Service Bus event publishing code in template solution corrected.

## v2.0.0
- **Note:** This version contains a number of breaking changes and generated artefact file name changes; all existing generated artefacts should be deleted and re-generated. Where possible it is recommended to create new using the template solution and copy across existing non-generated artefacts, such as the NTangle YAML etc.
- *Enhancement:* Removed need for `UseSqlServer` for `CodeGenConsole` as this is the only supported database option. New `UseDeploymentOption` to enable deployment option override; defaults to `DeploymentOption.DbEx`.
- *Enhancement:* Leverage `EventDataFormatter` and `IEventPublisher.CreateValueEvent` for improved _CoreEx_ consistency.
- *Enhancement:* `VersionTracking` table has had `Object` column replaced with `Schema` and `Table` columns to be explicit/consistent.
- *Enhancement:* Improved configuration override capabilities using `appsettings.json`.
- *Enhancement:* Template solution has refactored baseline code output.
- *Enhancement:* Updated `CoreEx` (`v2.10.1`) and `DbEx` (`v2.3.4`) dependencies.
- *Fixed:* Create and update events not published where a subsequent delete occurs; i.e. the entity is physically deleted. The create and update cannot be published as the entity contents are no longer available, and there is not way to reliable way to construct.
- *Fixed:* Previously where a create (optional updates) and delete occured within a batch no events were published; now the delete event is published as a minimum to be consistent with the preceeding fix.
- *Fixed:* Code-generated entities correctly invoke `IdentifierMapping` asynchronously using an `await`.

## v1.1.0
- *Enhancement:* Updated `CoreEx` (`v2.5.1`) and `DbEx` (`v2.3.2`); this results in minor breaking changes that will need to be addressed.
- [*Issue 25:*](https://github.com/Avanade/NTangle/issues/25) Resolved code generation error where a database table name resulted in an alias of `r`.
- [*Issue 26:*](https://github.com/Avanade/NTangle/issues/26) Resolved duplication of models when generating code.

## v1.0.8
- *Enhancement:* Updated `CoreEx` and `DbEx`; key changes related to: `IPrimaryKey` and event outbox service ownership.  

## v1.0.7
- *Fixed:* Event Outbox dequeue and send publishing failure will log and continue (allowing retry); versus fail process.
- *Enhancement:* Template updated to show code (commented out) for Azure Service Bus event publishing.

## v1.0.6
- *Enhancement:* Updated `DbEx` dependency to latest.

## v1.0.5
- *Enhancement:* Updated `CoreEx` and `DbEx` dependencies to latest. This is to account for the move of the common ADO-based database access logic from `DbEx` to `CoreEx`.

## v1.0.4
- *Enhancement:* Updated `CoreEx`, `OnRamp` and `DbEx` dependencies; including other non-Avanade dependencies to latest.
- *Enhancement:* Template solution now outputs .NET 6.0 projects.

## v1.0.3
- *Enhancement:* **Breaking Changes** - Integrated latest `CoreEx` to replace the previous entity capabilities; integrated latest `DbEx` to provide the SQL Server Event Outbox capabilities. Also, leverages the in-process event outbox publishing to reduce message send latency, with the database event outbox acting as an audit/backup for resiliency.
- [*Issue 10:*](https://github.com/Avanade/NTangle/issues/10) Validation added to `JoinConfig` to ensure hierarchical cicrular references result in an appropriate error during code-generation; versus entering into an endless loop.

## v1.0.2
- [*Issue 7:*](https://github.com/Avanade/NTangle/issues/7) The `Table` object `Name` property can now be used to rename a table within an _NTangle_ context and must be unique. The `Table` property which automatically defaults from `Name`, is used to specify the actual database table name where different.
- [*Issue 7:*](https://github.com/Avanade/NTangle/issues/7) The `VersionTracking` table has been updated with a new column `Object` as a replacement for `Schema` and `Table` as the unique `Name` needs to be used. Will be formatted as `Schema_Name`.
- [*Issue 7:*](https://github.com/Avanade/NTangle/issues/7) The `Table` object now supports none or more `Where` conditions to filter the published data where applicable.
- *Enhancement:* All `EventData` properties are now persisted to the outbox, previously `Subject` and `Action` were not. The `EventData` column has been renamed to `Data` and will now only contain `Data` not the full `EventData` contents (no need to duplicate the other columns).

## v1.0.1
- *New:* Initial publish to GitHub. This was originally harvested from, and will replace, the core CDC code-generation and runtime within [Beef](https://github.com/Avanade/Beef).