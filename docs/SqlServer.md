# Microsoft SQL Server

This documents the _nTangle_ [architecture](#Architecture) and corresponding [code-generation](#Code-generation) when targeting a Microsoft SQL Server database. 

<br/>

## Architecture

The following represents the high-level conceptual run-time architecture for a solution leveraging _nTangle_.

![Logo](../images/Architecture.png "Architecture")

</br>

The key components are as follows.
- [**Orchestrator**](#Orchestrator) - one per entity (aggregate root) change tracking and event publisher orchestrator. The internal execution phases are depicted.
- [**Hosted service**](#Hosted-service) - one per entity (aggregate root) timer-based hosted service responsible for executing the orchestrator.
- [**Outbox dequeue publisher**](#Outbox-dequeue-publisher) - event outbox dequeue and publishing. The internal execution phases are depicted.
- [**Outbox dequeue hosted service**](#Outbox-dequeue-hosted-service) - timer-based hosted service responsible for executing the outbox dequeue publisher.

<br/>

### Orchestrator

The [`XxxCdcOrchestrator`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Data/Generated/ContactCdcOrchestrator.cs) is responsible for the primary orchestration of the CDC-related change tracking and event publishing - this is essentially the secret sauce behind _nTangle_. This is achieved by executing (orchestrating) the following phases in the order specifed.

- [Change detection](#Change-detection) - detect changes to the underlying tables and package within a batch;
- [Consolidation](#Consolidation) - consolidate changes to minimize redundant event publishing;
- [Identifier mapping](#Identifier-mapping) - assign global identifier mappings;
- [Versioning](#Versioning) - version events to minimize publishing with same content;
- [Publishing](#Publishing) - publish / send events to selected destination;
- [Completion](#Completion) - complete batch and record latest version hashes.

The [`XxxCdcOrchestrator`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Data/Generated/ContactCdcOrchestrator.cs) inherits from
 [`EntityOrchestrator`](../src/NTangle/Cdc/EntityOrchestrator.cs), which in turn inherits from [`EntityOrchestratorBase`](../src/NTangle/Cdc/EntityOrchestratorBase.cs), to enable the standardized orchestration processing.

An orchestrator is generated per [entity](../src/NTangle/IEntity.cs) (the aggregate root which has a naming convention of [`XxxCdc`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Entities/Generated/ContactCdc.cs)). The entity is essentially the .NET representation of the configured root and child table hierarchy, and selected columns as properties.

<br/>

#### Change detection

The orchestrator will select (detect) the changes as enabled by the SQL Server CDC capabilities. This phase is achieved by invoking the [`spXxxBatchExecute`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Stored%20Procedures/Generated/spContactBatchExecute.sql) stored procedure and updating the result sets into the .NET entity equivalents. The following stored procedure steps are all performed within a database transactional context.

Step | Description
-|-
Batch check | Check if there is already an incomplete Batch and attempt to reprocess; otherwise, create a new Batch (only where data is found to be processed). The batch tracking is persisted in the [XxxBatchTracking](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Tables/Generated/ContactBatchTracking.sql) table.
LSN check | Get minimum and maximum LSNs for each table. If the minimum is less than previous Batch minimum then there is a CDC data loss scenario and some changes will be lost as a result; this will error unless option to continue with data loss is selected.
Root CDC | Gets all CDC changes (create, update and delete) for the table (see [`fn_cdc_get_all_changes_`](https://docs.microsoft.com/en-us/sql/relational-databases/system-functions/cdc-fn-cdc-get-all-changes-capture-instance-transact-sql)) up to the maximum query size. Store the result into a temporary `#changes` table.
Child CDC | Gets all CDC changes (create, update and delete) for each child table up to the maximum query size joining against the parent table(s) to ensure existence. Append the result into the temporary `#changes` table where distinct (as per root table primary key).
Batch query | Select result set for the latest Batch details (will be in an incomplete state).
Root query | Select result set using the temporary `#changes` table to left outer join to the root table (latest), then left outer join to [`VersionTracking`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Tables/Generated/VersionTracking.sql) (last version hash) and [`IdentifierMapping`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Tables/Generated/IdentifierMapping.sql) (for selected colums where configured).
Child query | Select result set using the temporary `#changes` for each child table using inner join to ensure data is selected for only what currently exists within the database (latest), then left outer join to [`IdentifierMapping`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Tables/Generated/IdentifierMapping.sql) (for selected colums where configured).

</br>

#### Consolidation

The orchestrator will consolidate the selected data returned from the [change detection](#Change-detection) phase. The following consolidations are performed for each distinct _primary key_ from the root table (being the aggregated entity), in order specified. The result of the consolidation phase is that there should be zero or more records (one per _primary key_ from the root table).

Step | Description
-|-
Skip (winless) | Where there is a create, zero or more updates, followed by a delete (in what is considered quick succession), then no publish will occur (as the end result is a redundant delete).
Delete wins | Where there are one or more updates, followed by a delete, then only the delete will be considered for publishing.
Create wins | Where there is a create, followed by one or more updates, then only the create will be considered for publishing.

Where a delete is referenced above, this relates to both physical and logical deletes equally (as per configuration).

</br>

#### Identifier mapping

The orhestrator (where identifier mapping is configured) will assign new global identifiers generated using [`IIdentifierGenerator<T>`](../src/NTangle/IIdentifierGenerator.cs) to each of the selected columns where no value was previously selected during the [change detection](#Change-detection) phase. The [`spIdentifierMappingCreate`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Stored%20Procedures/Generated/spIdentifierMappingCreate.sql) stored procedure is then invoked to persist the mappings into the [`IdentifierMapping`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Tables/Generated/IdentifierMapping.sql) table; where mapping is already assigned, then the previously assigned value will be returned for use.

</br>

#### Versioning

The orchestrator will version each record by JSON serializing the data (removing any properties that should be excluded) and then [SHA256](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha256) hashed. This is compared to the existing version (from the [`VersionTracking`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Tables/Generated/VersionTracking.sql) table) selected during the [change detection](#Change-detection) phase. Where the version has not changed for the record then no publish will occur; avoids sending same event content more than once.

</br>

#### Publishing

The orchestrator will instantiate an [`EventData`](../src/NTangle/Events/EventData.cs) per record, then invoke the [`IEventPublisher.SendAsync()`](../src/NTangle/Events/IEventPublisher.cs) passing all events to be published.

The default is to use the [`OutboxEventPublisher`](../src/NTangle/Events/OutboxEventPublisherBase.cs) which enqueues all events using stored procedure [`spEventOutboxEnqueue`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Stored%20Procedures/Generated/spEventOutboxEnqueue.sql) into the [`EventOutbox`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Tables/Generated/EventOutbox.sql) and [`EventOutboxData`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Tables/Generated/EventOutboxData.sql) tables. This can be configured to use an alternate `IEventPublisher` where required.

</br>

#### Completion

The orchestrator will complete the batch and update the latest version tracking hashes. This phase is managed by the [`spXxxBatchComplete`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Stored%20Procedures/Generated/spContactBatchComplete.sql)  stored procedure. The following stored procedure steps are all performed within a database transactional context.

Step | Description
-|-
Batch complete | Update the batch as complete with the [XxxBatchTracking](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Tables/Generated/ContactBatchTracking.sql) table, including updating the completed date/time.
Version tracking | Create or update (merge) the latest versions for each of the records published into the [`VersionTracking`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Tables/Generated/VersionTracking.sql) table.

<br/>

### Hosted service

The [`XxxHostedService`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Services/Generated/ContactHostedService.cs) is responsible for hosting the [Orchestrator](#Orchestrator) at runtime. This is a [timer-based service](../src/NTangle/Services/TimerHostedServiceBase.cs) which will continue to execute on a configured interval until the process host is stopped.

<br/>

### Outbox dequeue publisher

The [`OutboxDequeuePublisher`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Events/Generated/OutboxDequeuePublisher.cs) is responsible for managing the dequeue of events from the database and then publishing. This is achieved by executing the following phases in the order specifed.

- [Dequeue](#Dequeue) - dequeue one or more events from the outbox;
- [Publishing](#publishing2) - publish / send events to selected destination.

This phases are encapsulated within a database transaction to ensure that the publishing completes successfully before the dequeue is committed. As a result this will ensure guaranteed delivery, but may result in messages being sent more than once. The event receiver may be required to perform duplicate detection on the event where duplicates can not be tolerated.

The [`OutboxDequeuePublisher`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Events/Generated/OutboxDequeuePublisher.cs) inherits from
 [`OutboxDequeuePublisherBase`](../src/NTangle/Data/OutboxDequeuePublisherBase.cs), which implements [`IOutboxDequeuePublisher`](../src/NTangle/Data/IOutboxDequeuePublisher.cs), to enable the standardized dequeue and publish.

<br/>

#### Dequeue

The events will be dequeued from the database using stored procedure [`spEventOutboxDequeue`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Stored%20Procedures/Generated/spEventOutboxDequeue.sql); this will dequeue the events  from the underlying [`EventOutbox`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Tables/Generated/EventOutbox.sql) and [`EventOutboxData`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Tables/Generated/EventOutboxData.sql) tables.

<br/>

#### Publishing
<a id="publishing2"/>

The publisher must implement `IOutboxEventPublisher`. The underlying `IOutboxEventPublisher.SendAsync()` method will be invoked passing the dequeued events for publishing / sending to the selected destination. Once all events have been successfully sent the database transaction will be committed.

<br/>

### Outbox dequeue hosted service

The [`OutboxDequeueHostedService`](../src/NTangle/Services/OutboxDequeueHostedService.cs) is responsible for hosting the [Outbox dequeue publisher](#Outbox-dequeue-publisher) at runtime. This is a [timer-based service](../src/NTangle/Services/TimerHostedServiceBase.cs) which will continue to execute on a configured interval until the process host is stopped.

<br/>

## Code-generation

The [code-generator](../tools/NTangle.Template/content/AppName.CodeGen/Program.cs) will leverage the [ntangle.yaml](../tools/NTangle.Template/content/AppName.CodeGen/ntangle.yaml) configuration to generate both [Database](#Database) and [.NET](#dotnet) artefacts.

Where `Xxx` is referenced in the artefact name this is to be replaced with the name of the entity (root aggregate). Also, the artefact name represents the default, there are opportunities within the `ntangle.yaml` to change anumber of these where applicable.

<br/>

### Database

The `AppName.Database` project generated artefacts are as follows.

Type | Artefact | Description
-|-|-
Script | [`CdcEnable`](../samples/SqlServerDemo/SqlServerDemo.Database/Generated/CdcEnable.sql) | Turns CDC on for the selected tables.
Schema | [`NTangle`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Generated/NTangle.sql) | Creates the database schema.
Stored procedure | [`spXxxBatchComplete`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Stored%20Procedures/Generated/spContactBatchComplete.sql) | Performs the batch completion for an entity.
Stored procedure | [`spXxxBatchExecute`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Stored%20Procedures/Generated/spContactBatchExecute.sql) | Performs the batch execution for an entity.
Stored procedure | [`spEventOutboxDequeue`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Stored%20Procedures/Generated/spEventOutboxDequeue.sql) | Performs the event outbox dequeue.
Stored procedure | [`spEventOutboxEnqueue`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Stored%20Procedures/Generated/spEventOutboxEnqueue.sql) | Performs the event outbox enqueue.
Stored procedure | [`spIdentifierMappingCreate`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Stored%20Procedures/Generated/spIdentifierMappingCreate.sql) | Performs the indentifier mapping creation.
Table | [`XxxBatchTracking`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Tables/Generated/ContactBatchTracking.sql) | Batch tracking.
Table | [`EventOutbox`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Tables/Generated/EventOutbox.sql) | Event outbox.
Table | [`EventOutboxData`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Tables/Generated/EventOutboxData.sql) | Event outbox data.
Table | [`IdentifierMapping`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Tables/Generated/IdentifierMapping.sql) | Identifier mapping.
Table | [`VersionTracking`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Tables/Generated/VersionTracking.sql) | Version (hash) tracking.
UDT | [`udtEventOutboxList`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Tables/Generated/VersionTracking.sql) | Event outbox user-defined type.
UDT | [`udtEventOutboxList`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Types/User-Defined%20Table%20Types/Generated/udtEventOutboxList.sql) | Event outbox list user-defined type.
UDT | [`udtIdentifierMappingList`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Types/User-Defined%20Table%20Types/Generated/udtIdentifierMappingList.sql) | Version tracking list user-defined type.
UDT | [`udtVersionTrackingList`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Types/User-Defined%20Table%20Types/Generated/udtVersionTrackingList.sql) | Version tracking list user-defined type.

<br/>

### .NET
<a id="dotnet"/>

The `AppName.Publisher` project .NET generated artefacts are as follows.

Namespace | Artefact | Description
-|-|-
<para/> | [`ServiceCollectionExtensions`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Generated/ServiceCollectionExtensions.cs) | [`IServiceCollection`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) extension methods for Dependency Injection (DI) set up.
Data | [`XxxCdcOrchestractor`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Data/Generated/ContactCdcOrchestrator.cs) | Change tracking and event publisher orchestrator.
Data | [`EventOutboxMapper`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Data/Generated/EventOutboxMapper.cs) | Event outbox database to .NET mapper.
Data | [`IdentifierMappingMapper`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Data/Generated/IdentifierMappingMapper.cs) | Identifier mapping database to .NET mapper.
Data | [`VersionTrackingMapper`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Data/Generated/VersionTrackingMapper.cs) | Version tracking database to .NET mapper.
Entities | [`XxxCdc`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Entities/Generated/ContactCdc.cs) | Entity (aggregate root) representation of database table(s) and relationships.
Events | [`OutboxDequeuePublisher`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Events/Generated/OutboxDequeuePublisher.cs) | Manages the dequeue of events and publishing thereof.
Events | [`OutboxEventPublisher`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Events/Generated/OutboxEventPublisher.cs) | Publishes events to the event outbox tables within the database.
Services | [`XxxHostedService`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Services/Generated/ContactHostedService.cs) | Timer-based host for the `XxxCdcOrestrator`.
