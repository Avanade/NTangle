# Microsoft SQL Server

This documents the _nTangle_ [architecture](#Architecture) and corresponding [code-generation](#Code-generation) when targeting a Microsoft SQL Server database. 

<br/>

## Overview

At its core _NTangle_ is a database-driven code-generation solution, in that it leverages an existing database to infer its underlying schema (tables and columns). A YAML (or JSON) configuration file is required to define the CDC requirements, being the root and child tables, and their relationships, representing the entity (aggregate root). The _nTangle_ code-gen tool connects to the database, and using the YAML configuration generates the required database and .NET publishing runtime artefacts. This publishing runtime then uses the database's CDC capabilities to detect changes to the underlying tables and trigger the creation of the related entity events that are then published.

![Overview](../images/Overview.png "Overview")

The core components are:
- **Database** - the existing tables plus generated _nTangle_ runtime artefacts (including outbox);
- **Config** - the _nTangle_ code-gen configuration;
- **Tooling** - the _nTangle_ code generator and resulting generated publisher .NET runtime;
- **Events** - the resulting published entity event (depicted as a [CloudEvent](https://cloudevents.io/)).

<br/>

## Change-data-capture (CDC)

This official [documentation](https://docs.microsoft.com/en-us/sql/relational-databases/track-changes/about-change-data-capture-sql-server) describes the Microsoft SQL Server CDC-capabilities.

In addition, this [article](https://www.mssqltips.com/sqlservertip/5212/sql-server-temporal-tables-vs-change-data-capture-vs-change-tracking--part-2/) provides an excellent overview of the Microsoft SQL Server CDC-capabilities and walks through the process of setting up and using to aid in the fundamental understanding of this _key_ dependent capability.

<br/>

### Performance considerations

The introduction of CDC into a database can have an impact on the database performance, and as such it is important to consider this impact; see [performance considerations](https://learn.microsoft.com/en-us/sql/relational-databases/track-changes/about-change-data-capture-sql-server?view=sql-server-ver16#performance-considerations).

Within the _nTangle_ cofiguration the `CdcEnable` property can be used to enable/disable the automatic generation of the CDC-related `sys.sp_cdc_enable_table` TSQL code. This allows the developer and/or database administrator to manually enable CDC on the required tables external to _nTangle_ code generation. By default, the `CdcEnable` property is set to `false`.

Given the above, it is generally _recommended_ that CDC enablement is explicity managed by the developer and/or database administrator. Additionally, as schema changes are made to the underlying tables the CDC configuration may need to be changed accordingly; this does not happen automatically within SQL Server; see this article for more [information](https://www.mssqltips.com/sqlservertip/4096/understanding-how-dml-and-ddl-changes-impact-change-data-capture-in-sql-server/).

<br/>

### CDC vs Change Tracking

Microsoft SQL Server provides two change tracking capabilities, namely CDC and [Change Tracking](https://learn.microsoft.com/en-us/sql/relational-databases/track-changes/track-data-changes-sql-server#Tracking). Change Tracking differs in that it _only_ captures the fact that rows in a table were changed, but doesn't capture the data that was changed. Given the multiple related table hierarchy triggering that _nTangle_ enables the joining columns as well as the primary key columns are required to be captured; this is not possible with Change Tracking as it only captures the primary key columns; therefore, making it unsuitable for walking back up the join hierarchy to determine the primary entity (aggregate root) that was changed. And largely impossible where a child table row has been physically deleted.

<br/>

## Architecture

The following represents the high-level conceptual run-time architecture for a solution leveraging _nTangle_.

![Architecture](../images/Architecture.png "Architecture")

</br>

The key components are as follows.
- [**Orchestrator**](#Orchestrator) - one per entity (aggregate root) change tracking and event publisher orchestrator (execution phases depicted).
- [**Hosted service**](#Hosted-service) - one per entity (aggregate root) timer-based hosted service responsible for executing the orchestrator (optional).
- [**Outbox dequeue publisher**](#Outbox-dequeue-publisher) - event outbox dequeue and publishing (execution phases depicted).
- [**Outbox dequeue hosted service**](#Outbox-dequeue-hosted-service) - timer-based hosted service responsible for executing the outbox dequeue publisher.

<br/>

### Orchestrator

The [`XxxOrchestrator`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Data/Generated/ContactOrchestrator.cs) is responsible for the primary orchestration of the CDC-related change tracking and event publishing - this is essentially the "secret sauce" behind _nTangle_. This is achieved by executing (orchestrating) the following phases in the order specifed.

- [Change detection](#Change-detection) - detect changes to the underlying tables and package within a batch;
- [Consolidation](#Consolidation) - consolidate changes to minimize redundant event publishing;
- [Identifier mapping](#Identifier-mapping) - assign global identifier mappings;
- [Versioning](#Versioning) - version events to minimize publishing with same content;
- [Publishing](#Publishing) - publish / send events to selected destination;
- [Completion](#Completion) - complete batch and record latest version hashes.

The [`XxxOrchestrator`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Data/Generated/ContactOrchestrator.cs) inherits from
 [`EntityOrchestrator`](../src/NTangle/Cdc/EntityOrchestrator.cs), which in turn inherits from [`EntityOrchestratorBase`](../src/NTangle/Cdc/EntityOrchestratorBase.cs), to enable the standardized orchestration processing.

An orchestrator is generated per [entity](../src/NTangle/IEntity.cs) (the aggregate root which has a naming convention of [`XxxCdc`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Entities/Generated/ContactCdc.cs)). The entity is essentially the .NET representation of the configured root and child table hierarchy, and selected columns as properties.

<br/>

#### Change detection

The orchestrator will select (detect) the changes as enabled by the SQL Server CDC capabilities. This phase is achieved by invoking the [`spXxxBatchExecute`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Stored%20Procedures/Generated/spContactBatchExecute.sql) stored procedure and updating the result sets into the .NET entity equivalents. The following stored procedure steps are all performed within a database transactional context.

Step | Description
-|-
Batch check | Check if there is already an incomplete Batch and attempt to reprocess; otherwise, create a new Batch (only where data is found to be processed). The batch tracking is persisted in the [XxxBatchTracking](../samples/SqlServerDemo/SqlServerDemo.Database/Migrations/20221107-232848-03-create-ntangle-contactbatchtracking-table.sql) table.
LSN check | Get minimum and maximum LSNs for each table. If the minimum is less than previous Batch minimum then there is a CDC data loss scenario and some changes will be lost as a result; this will error unless option to continue with data loss is selected.
Root CDC | Gets all CDC changes (create, update and delete) for the table (see [`fn_cdc_get_all_changes_`](https://docs.microsoft.com/en-us/sql/relational-databases/system-functions/cdc-fn-cdc-get-all-changes-capture-instance-transact-sql)) up to the maximum query size. Store the result into a temporary `#changes` table.
Child CDC | Gets all CDC changes (create, update and delete) for each child table up to the maximum query size joining against the parent table(s) to ensure existence. Append the result into the temporary `#changes` table where distinct (as per root table primary key).
Batch query | Upsert batch data and then select result set for the latest Batch details (will be in an incomplete state).
Root query | Select result set using the temporary `#changes` table to left outer join to the root table (latest), then left outer joins to [`VersionTracking`](../samples/SqlServerDemo/SqlServerDemo.Database/Migrations/20221107-232848-02-create-ntangle-versiontracking-table.sql) (last version hash) and [`IdentifierMapping`](../samples/SqlServerDemo/SqlServerDemo.Database/Migrations/20221107-232848-07-create-ntangle-identifiermapping-table.sql) (for selected colums where configured).
Child query | Select result set using the temporary `#changes` for each child table using inner joins to ensure data is selected for only what currently exists within the database (latest), then left outer joins to [`IdentifierMapping`](../samples/SqlServerDemo/SqlServerDemo.Database/Migrations/20221107-232848-07-create-ntangle-identifiermapping-table.sql) (for selected colums where configured).

</br>

#### Consolidation

The orchestrator will consolidate the selected data returned from the [change detection](#Change-detection) phase. The following consolidations are performed for each distinct _primary key_ from the root table (being the aggregated entity), in order specified. The result of the consolidation phase is that there should be zero or more records (one per _primary key_ from the root table).

Step | Description
-|-
Delete wins | Where there is a created and/or, one or more updates, followed by a delete, then only the delete will be considered for publishing. Can not include intermediary create/update events as the data is no longer available (physically deleted).
Create wins | Where there is a create, followed by one or more updates, then only the create will be considered for publishing.

Where a delete is referenced above, this relates to both physical and logical deletes equally (as per configuration).

</br>

#### Identifier mapping

The orhestrator (where identifier mapping is configured) will assign new global identifiers generated using [`IIdentifierGenerator<T>`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/IIdentifierGenerator.cs) to each of the selected columns where no value was previously selected during the [change detection](#Change-detection) phase.

The [`spIdentifierMappingCreate`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Stored%20Procedures/Generated/spIdentifierMappingCreate.sql) stored procedure is then invoked to persist the mappings into the [`IdentifierMapping`](../samples/SqlServerDemo/SqlServerDemo.Database/Migrations/20221107-232848-07-create-ntangle-identifiermapping-table.sql) table; where mapping is already assigned, then the previously assigned value will be returned for use and the newly allocated value discarded. This may occur where concurrent access is being performed against the same related identifier.

</br>

#### Versioning

The orchestrator will version each record by JSON serializing the data (removing any properties that should be excluded) and then [SHA256](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha256) hashed. This is compared to the existing version (from the [`VersionTracking`](../samples/SqlServerDemo/SqlServerDemo.Database/Migrations/20221107-232848-02-create-ntangle-versiontracking-table.sql) table) selected during the [change detection](#Change-detection) phase. Where the version has not changed for the record then no publish will occur; avoids sending same event content more than once.

</br>

#### Publishing

The orchestrator will instantiate an [`EventData`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/EventData.cs) per record, then invoke the [`IEventPublisher.Publish()`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/IEventPublisher.cs) passing all events to be published.

The default is to use the generated [`EventOutboxEnqueue`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Data/Generated/EventOutboxEnqueue.cs) which enqueues all events using stored procedure [`spEventOutboxEnqueue`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/Outbox/Stored%20Procedures/Generated/spEventOutboxEnqueue.sql) into the [`EventOutbox`](../samples/SqlServerDemo/SqlServerDemo.Database/Migrations/20221107-232848-05-create-outbox-eventoutbox-table.sql) and [`EventOutboxData`](../samples/SqlServerDemo/SqlServerDemo.Database/Migrations/20221107-232848-06-create-outbox-eventoutboxdata-table.sql) tables. This can be configured to use an alternate `IEventPublisher` where required.

There are multiple advantages of using the event outbox, a) a log of published events will be maintained, b) performance of orchestrator may be improved, c) dependency of external destination removed from orchestrator, and d) multiple sends will have the same event identifier which is useful in duplicate detection scenarios guaranteeing at least once delivery.

</br>

#### Completion

The orchestrator will complete the batch and update the latest version tracking hashes. This phase is managed by the [`spXxxBatchComplete`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Stored%20Procedures/Generated/spContactBatchComplete.sql) stored procedure. The following stored procedure steps are all performed within a database transactional context.

Step | Description
-|-
Batch complete | Update the batch as complete within the [XxxBatchTracking](../samples/SqlServerDemo/SqlServerDemo.Database/Migrations/20221107-232848-03-create-ntangle-contactbatchtracking-table.sql) table, including updating the completed date/time.
Version tracking | Create or update (merge) the latest versions for each of the records published into the [`VersionTracking`](../samples/SqlServerDemo/SqlServerDemo.Database/Migrations/20221107-232848-02-create-ntangle-versiontracking-table.sql) table.

<br/>

### Hosted service

The [`XxxHostedService`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Services/Generated/ContactHostedService.cs) is responsible for hosting the [Orchestrator](#Orchestrator) at runtime. This is a [synchronized timer-based hosted service](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Hosting/SynchronizedTimerHostedServiceBase.cs) which will continue to execute on a configured interval until the process host is stopped.

An [`IServiceSynchronizer`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Hosting/IServiceSynchronizer.cs) is required by the `XxxHostedService` to ensure that _only_ a single serivce per root table is running at any given time. This is primarily needed to ensure that the explicit order of changes in honored; otherwise, events for the root table would be emitted out of sequence.

The `XxxHostedService` is not explicitly required and can be replaced with any other host implementation, i.e. timer-based trigger Azure Function that directly invokes the `XxxOrchestrator`. As above, this host will need to manage the sychronized singleton behavior; see [`BlobLeaseSynchronizer`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Azure/Storage/BlobLeaseSynchronizer.cs).

<br/>

### Outbox dequeue publisher

The [`EventOutboxHostedService`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Database.SqlServer/Outbox/EventOutboxHostedService.cs) is responsible for managing the dequeue of events from the database and then sending. This is achieved by executing the following phases in the order specifed.

- [Dequeue](#Dequeue) - dequeue one or more events from the outbox;
- [Sending](#sending) - send events to selected destination.

The phases are encapsulated within a database transaction to ensure that the publishing completes successfully before the dequeue is committed. As a result this will ensure guaranteed delivery, but may result in messages being sent more than once. The event receiver may be required to perform duplicate detection on the events where duplicates can not be tolerated.

The [`EventOutboxHostedService`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Database.SqlServer/Outbox/EventOutboxHostedService.cs) property `EventOutboxDequeueFactory` must be set to instantiate an instance of the [`EventOutboxDequeueBase`](https://github.com/Avanade/DbEx/blob/main/src/DbEx/SqlServer/EventOutboxDequeueBase.cs). This instance is the generated class [`EventOutboxDequeue`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Data/Generated/EventOutboxDequeue.cs), which in turn manages the final [`IEventSender`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/IEventSender.cs) to send the events to the final destination.

<br/>

#### Dequeue

The events will be dequeued (up to the maximum dequeue size) from the database using stored procedure [`spEventOutboxDequeue`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/Outbox/Stored%20Procedures/Generated/spEventOutboxDequeue.sql); this will dequeue the events from the underlying [`EventOutbox`](../samples/SqlServerDemo/SqlServerDemo.Database/Migrations/20221107-232848-05-create-outbox-eventoutbox-table.sql) and [`EventOutboxData`](../samples/SqlServerDemo/SqlServerDemo.Database/Migrations/20221107-232848-06-create-outbox-eventoutboxdata-table.sql) tables.

<br/>

#### Sending

The [`EventOutboxDequeue`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Data/Generated/EventOutboxDequeue.cs) manages the final [`IEventSender`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/IEventSender.cs) to send the events to the final destination.

To leverage the likes of Azure Service Bus consider the _CoreEx_ [`ServiceBusSender`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Azure/ServiceBus/ServiceBusSender.cs).

<br/>

### Outbox dequeue hosted service

The [`EventOutboxHostedService`](../src/NTangle/Services/EventOutboxHostedService.cs) is responsible for hosting the [Outbox dequeue publisher](#Outbox-dequeue-publisher) at runtime. This is a [synchronized timer-based service](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Hosting/SynchronizedTimerHostedServiceBase.cs) which will continue to execute on a configured interval until the process host is stopped.

<br/>

## Code-generation

The [code-generator](../tools/NTangle.Template/content/AppName.CodeGen/Program.cs) will leverage the [ntangle.yaml](../tools/NTangle.Template/content/AppName.CodeGen/ntangle.yaml) configuration to generate both [Database](#Database) and [.NET](#dotnet) artefacts.

Where `Xxx` is referenced in the artefact name this is replaced with the name of the entity (root aggregate). Also, the artefact name represents the default, there are opportunities within the `ntangle.yaml` to change a number of these where applicable.

Depending on the whether using _DbEx_ or _DACPAC_ for managing the Database schema and deployment then the naming and placement of some artefacts may differ. Finally, features such as _event outbox_ and _identity mapping_ are configurable. Where not leveraged their respective artefacts will not be generated. 

<br/>

### Database

The `AppName.Database` project generated artefacts are as follows.

Type | Artefact | Description
-|-|-
Script | [`CdcEnable`](../samples/SqlServerDemo/SqlServerDemo.Database/Migrations/CdcEnable.post.deploy.sql) | Turns CDC on for the selected tables.
Schema | [`NTangle`](../samples/SqlServerDemo/SqlServerDemo.Database/Migrations/20221107-232848-01-create-ntangle-schema.sql) | Creates the database schema.
Stored procedure | [`spXxxBatchComplete`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Stored%20Procedures/Generated/spContactBatchComplete.sql) | Performs the batch completion for an entity.
Stored procedure | [`spXxxBatchExecute`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Stored%20Procedures/Generated/spContactBatchExecute.sql) | Performs the batch execution for an entity.
Stored procedure | [`spEventOutboxDequeue`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/Outbox/Stored%20Procedures/Generated/spEventOutboxDequeue.sql) | Performs the event outbox dequeue.
Stored procedure | [`spEventOutboxEnqueue`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/Outbox/Stored%20Procedures/Generated/spEventOutboxEnqueue.sql) | Performs the event outbox enqueue.
Stored procedure | [`spIdentifierMappingCreate`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Stored%20Procedures/Generated/spIdentifierMappingCreate.sql) | Performs the indentifier mapping creation.
Table | [`XxxBatchTracking`](../samples/SqlServerDemo/SqlServerDemo.Database/Migrations/20221107-232848-03-create-ntangle-contactbatchtracking-table.sql) | Batch tracking.
Schema | [`EventOutbox`](../samples/SqlServerDemo/SqlServerDemo.Database/Migrations/20221107-232848-04-create-outbox-eventoutbox-schema.sql) | Creates the Event outbox database schema.
Table | [`EventOutbox`](../samples/SqlServerDemo/SqlServerDemo.Database/Migrations/20221107-232848-05-create-outbox-eventoutbox-table.sql) | Event outbox.
Table | [`EventOutboxData`](../samples/SqlServerDemo/SqlServerDemo.Database/Migrations/20221107-232848-06-create-outbox-eventoutboxdata-table.sql) | Event outbox data.
Table | [`IdentifierMapping`](../samples/SqlServerDemo/SqlServerDemo.Database/Migrations/20221107-232848-07-create-ntangle-identifiermapping-table.sql) | Identifier mapping.
Table | [`VersionTracking`](../samples/SqlServerDemo/SqlServerDemo.Database/Migrations/20221107-232848-02-create-ntangle-versiontracking-table.sql) | Version (hash) tracking.
UDT | [`udtEventOutboxList`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/Outbox/Types/User-Defined%20Table%20Types/Generated/udtEventOutboxList.sql) | Event outbox list user-defined type.
UDT | [`udtIdentifierMappingList`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Types/User-Defined%20Table%20Types/Generated/udtIdentifierMappingList.sql) | Identifier mapping list user-defined type.
UDT | [`udtVersionTrackingList`](../samples/SqlServerDemo/SqlServerDemo.Database/Schema/NTangle/Types/User-Defined%20Table%20Types/Generated/udtVersionTrackingList.sql) | Version tracking list user-defined type.

<br/>

### .NET
<a id="dotnet"/>

The `AppName.Publisher` project .NET generated artefacts are as follows.

Namespace | Artefact | Description
-|-|-
<para/> | [`ServiceCollectionExtensions`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Generated/IServiceCollectionExtensions.cs) | [`IServiceCollection`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) extension methods for Dependency Injection (DI) set up.
Data | [`XxxOrchestrator`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Data/Generated/ContactOrchestrator.cs) | Change tracking and event publisher orchestrator.
Data | [`EventOutboxDequeue`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Data/Generated/EventOutboxDequeue.cs) | Event outbox dequeue.
Data | [`EventOutboxEnqueue`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Data/Generated/EventOutboxEnqueue.cs) | Event outbox enqueue.
Data | [`IdentifierMappingMapper`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Data/Generated/IdentifierMappingMapper.cs) | Identifier mapping database to .NET mapper.
Data | [`VersionTrackingMapper`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Data/Generated/VersionTrackingMapper.cs) | Version tracking database to .NET mapper.
Entities | [`XxxCdc`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Entities/Generated/ContactCdc.cs) | Entity (aggregate root) representation of database table(s) and relationships.
Services | [`XxxHostedService`](../samples/SqlServerDemo/SqlServerDemo.Publisher/Services/Generated/ContactHostedService.cs) | Timer-based host for the `XxxCdcOrchestrator`.