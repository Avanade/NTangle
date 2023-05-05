# Change log

Represents the **NuGet** versions.

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