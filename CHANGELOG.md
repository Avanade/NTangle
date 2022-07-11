# Change log

Represents the **NuGet** versions.

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