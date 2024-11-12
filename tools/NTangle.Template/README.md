# NTangle.Template

This is the .NET template used to accelerate the creation of an _nTangle_ solution and all projects using `dotnet new`. This leverages the .NET Core [templating](https://docs.microsoft.com/en-au/dotnet/core/tools/custom-templates) functionality.

This article covers the following:
- [Installation](#Installation) - how to install the template.
- [Create solution](#Create-solution) - how to create the solution.
- [Demonstration](#Demonstration) - demonstates (walks-through) end-to-end creation and execution.

<br/>

## Installation

Before the `NTangle.Template` template can be used it must be installed from [NuGet](https://www.nuget.org/packages/NTangle.Template/). The `dotnet new install` command is used to perform this.

```
-- Use the latest published from NuGet...
dotnet new install ntangle.template --nuget-source https://api.nuget.org/v3/index.json

-- Or alternatively, point to a local folder...
dotnet new install ntangle.template --nuget-source C:\Users\Name\nuget-publish
```

To verify once installed, execute `dotnet new list ntangle`; this will output the following.

```
These templates matched your input: 'ntangle'

Template Name     Short Name  Language  Tags
----------------  ----------  --------  -------------------------------------
NTangle solution  ntangle     [C#]      NTangle/CDC/Database/Console/Solution
```

<br/>

## Create solution

To create the _Solution_ you must first be in the directory that you intend to create the artefacts within. The directory name is then used as the default for the application name.

The `dotnet new` command is used to create the initial solution artefacts that will leverage Microsoft SQL Server. Additionally, a `publisher` option can be specified to indicate what type of _Publisher_ project should be created; the values are `Console` (default), `Function` ([Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview)), or `None`.

```
dotnet new ntangle
dotnet new ntangle -publisher Console
dotnet new ntangle -publisher Function
```

<br/>

### What is created?

The following solution and projects will be created within the root `AppName` folder.

```
└── AppName
  └── AppName.CodeGen     # Code generation console (tooling)
  └── AppName.Database    # Source database console (tooling)
  └── AppName.Publisher   # Runtime orchestration and event publishing console
  └── AppName.SidecarDb   # Sidecar database console (tooling)
  └── AppName.sln         # Solution file that references all above projects
```

The solution and projects created contain all the requisite .NET Classes and NuGet references to build an _NTangle_ solution.

**Note:** the solution will **not** initially compile. There are references within the [`AppName.Publisher/Program.cs`](./content/AppName.CodeGen/Program.cs) that do not exist; these need to be created/generated.

The templated structure represents the bare minimum needed to start. Generally, the following projects are maintained in the sequence as follows.

- [`AppName.Database`](#AppNameDatabase)
- [`AppName.CodeGen`](#AppNameCodeGen)
- [`AppName.SidecarDb`](#AppNameSidecarDb)
- [`AppName.Publisher`](#AppNamePublisher)

<br/>

### AppName.Database

This represents the _source_ database project that will be used to maintain any additional database schema and data, depending on what additional capability is required. This is not a mandatory project, and can be removed if not required where leveraging alternate database management tools for the existing _source_ database.

The _source_ connection string defaulted within the [`Program.cs`](./content/AppName.Database/Program.cs) needs to be validated and adjusted to that required for development purposes.

Execute `dotnet run -- --help` to see the available command line arguments. 

The console application is generated with a reference to _DbEx_ (please review this [repo](https://github.com/Avanade/dbex) to further understand the database migration deployment capabilities). 

<br/>

### AppName.CodeGen

This represents the code generation project that will be used to generate the _nTangle_ artefact from the _source_ database.

The _source_ connection string defaulted within the [`Program.cs`](./content/AppName.CodeGen/Program.cs) needs to be validated and adjusted to that of the _source_ database.

Execute `dotnet run -- --help` to see the available command line arguments. 

The **key** artefact that is maintained is the [`ntangle.yaml`](./content/AppName.CodeGen/ntangle.yaml) which contains the CDC-related table configuration used to drive the code-generation. The contents are provided as an example only, and generally would be removed and replaced with the _actual_ configuration.

To execute the code-generation use `dotnet run`, or execute the `AppName.CodeGen.exe` directly.

<br/>

### AppName.SidecarDb

This represents the _sidecar_ database project that will be used to maintain the database migration scripts and schema objects that are required to support the _Publisher_ project.

A console application is generated with a reference to _DbEx_ (please review this [repo](https://github.com/Avanade/dbex) to further understand the database migration deployment capabilities).

The _sidecar_ connection string defaulted within the [`Program.cs`](./content/AppName.CodeGen/Program.cs) needs to be validated and adjusted to that required for development purposes. 

The required artefacts are generally added to this project using the earlier code-generator (see [`AppName.CodeGen`](#AppNameCodeGen)).

To execute the database migration use `dotnet run all`, or execute the `AppName.Database.exe all` directly.

<br/>

### AppName.Publisher

This default publisher is a console application whose purpose is to orchestrate the CDC processing and corresponding event processing leveraging all of the generated artefacts, both in the _sidecar_ database, and within the publisher itself.

The connection strings defaulted within the [`appsettings.json`](./content/AppName.Publisher/appsettings.json) needs to be validated and adjusted to that required for development purposes.

<br/>

## Demonstration

The following walks through the process of demonstrating how to execute the end-to-end functionality of _NTangle_. For the purposes of the demo the application name will be `FooBar`.

<br/>

### Create database

An existing database is required that contains tables and data. To create the database and set up, copy the contents of [`create-database.sql`](./create-database.sql) and execute using your favorite database tool. This will also turn on CDC for the database after it is created.

The following tables, with the relationships described, will be created.

```
Contact             // Root (aggregate)
└── Address         // Child 0:n - Zero--to-many addresses (e.g. Home and/or Postal)
  └── AddressType   // Child 1:1 - One-to-one address type (reference data)
```

_Tip:_ Where attempting for a subsequent time and the database already exists use the following to drop prior to recreating a fresh version.

```
USE master
ALTER DATABASE [FooBar] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
DROP DATABASE [FooBar]
```

<br/>

### Create solution

To start, create a new `FooBar` directory, change to that directory, and then create the solution using the _NTangle_ template. For the purposes of this demo that database artefacts will be managed by _DbEx_. Once created, open the solution in Visual Studio.

```
mkdir FooBar
cd FooBar
dotnet new ntangle
```

<br/>

### FooBar.CodeGen

The `ntangle.yaml` configuration file is largely pre-configured. For the purposes of this demo, the root property `cdcEnable` should be set to `true`; this will enable the generation of the TSQL to enable CDC on each of the tables.

The `Program.cs` has a default connection string, this needs to be validated and adjusted to that required to enable access to the required SQL Server instance.

Compile the application and execute directly from Visual Studio, or using `dotnet run`. The output from the console application should be similar to the following.

```
╔╗╔╔╦╗┌─┐┌┐┌┌─┐┬  ┌─┐  ╔═╗┌─┐┌┬┐┌─┐  ╔═╗┌─┐┌┐┌  ╔╦╗┌─┐┌─┐┬
║║║ ║ ├─┤││││ ┬│  ├┤   ║  │ │ ││├┤───║ ╦├┤ │││   ║ │ ││ ││
╝╚╝ ╩ ┴ ┴┘└┘└─┘┴─┘└─┘  ╚═╝└─┘─┴┘└─┘  ╚═╝└─┘┘└┘   ╩ └─┘└─┘┴─┘

FooBar.CodeGen Code Generation Tool.

Config = ntangle.yaml
Script = SqlServerDbExSidecar.yaml
OutDir = C:\ntangle-demo\FooBar
ExpectNoChanges = False
IsSimulation = False
Parameters:
  CreateDatabase = System.Func`2[System.String,CoreEx.Database.IDatabase]
  AppName = FooBar
  UseSidecar = True
Assemblies:
  FooBar.CodeGen, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
  NTangle.CodeGen, Version=3.0.0.0, Culture=neutral, PublicKeyToken=10b60143e92943c1
  DbEx.SqlServer, Version=2.7.1.0, Culture=neutral, PublicKeyToken=10b60143e92943c1
  DbEx, Version=2.7.1.0, Culture=neutral, PublicKeyToken=10b60143e92943c1
  Querying database to infer table(s)/column(s) schema...
    Database schema query complete [1147.6465ms]


Scripts:
 Template: SpExecuteBatch_sidecar_sql.hbs (TableCodeGenerator: SidecarDb/Schema/Xxx/Stored Procedures)
    Created -> C:\ntangle-demo\FooBar\FooBar.SidecarDb\Schema\NTangle\Stored Procedures\Generated\spContactBatchExecute.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SpTrackingBatch_sql.hbs (TableCodeGenerator: SidecarDb/Schema/Xxx/Stored Procedures)
    Created -> C:\ntangle-demo\FooBar\FooBar.SidecarDb\Schema\NTangle\Stored Procedures\Generated\spContactBatchTracking.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: ExecuteBatch_sql.hbs (TableCodeGenerator: Publisher/Resources)
    Created -> C:\ntangle-demo\FooBar\FooBar.Publisher\Resources\Generated\ContactExecuteBatch.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: ExecuteExplicit_sql.hbs (TableCodeGenerator: Publisher/Resources)
    Created -> C:\ntangle-demo\FooBar\FooBar.Publisher\Resources\Generated\ContactExecuteExplicit.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SpCompleteBatch_sql.hbs (TableCodeGenerator: SidecarDb/Schema/Xxx/Stored Procedures)
    Created -> C:\ntangle-demo\FooBar\FooBar.SidecarDb\Schema\NTangle\Stored Procedures\Generated\spContactBatchComplete.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SpResetBatch_sql.hbs (TableCodeGenerator: SidecarDb/Schema/Xxx/Stored Procedures)
    Created -> C:\ntangle-demo\FooBar\FooBar.SidecarDb\Schema\NTangle\Stored Procedures\Generated\spContactBatchReset.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SpIdentifierMappingCreate_sql.hbs (IdentifierMappingCodeGenerator: SidecarDb/Schema/Xxx/Stored Procedures)
  [Files: Unchanged = 0, Updated = 0, Created = 0]
 Template: SpEventOutboxEnqueue_sql.hbs (OutboxCodeGenerator: SidecarDb/Schema/Xxx/Stored Procedures)
    Created -> C:\ntangle-demo\FooBar\FooBar.SidecarDb\Schema\Outbox\Stored Procedures\Generated\spEventOutboxEnqueue.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SpEventOutboxDequeue_sql.hbs (OutboxCodeGenerator: SidecarDb/Schema/Xxx/Stored Procedures)
    Created -> C:\ntangle-demo\FooBar\FooBar.SidecarDb\Schema\Outbox\Stored Procedures\Generated\spEventOutboxDequeue.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: Entity_cs.hbs (TableCodeGenerator: Publisher/Entities)
    Created -> C:\ntangle-demo\FooBar\FooBar.Publisher\Entities\Generated\ContactCdc.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: EntityOrchestrator_cs.hbs (TableCodeGenerator: Publisher/Data)
    Created -> C:\ntangle-demo\FooBar\FooBar.Publisher\Data\Generated\ContactOrchestrator.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: EventOutboxEnqueue_cs.hbs (OutboxCodeGenerator: Publisher/Events)
    Created -> C:\ntangle-demo\FooBar\FooBar.Publisher\Data\Generated\EventOutboxEnqueue.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: EventOutboxDequeue_cs.hbs (OutboxCodeGenerator: Publisher/Events)
    Created -> C:\ntangle-demo\FooBar\FooBar.Publisher\Data\Generated\EventOutboxDequeue.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: IServiceCollectionExtensions_cs.hbs (RootCodeGenerator: Publisher)
    Created -> C:\ntangle-demo\FooBar\FooBar.Publisher\Generated\IServiceCollectionExtensions.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: EntityHostedService_cs.hbs (RootCodeGenerator: Publisher/Services)
    Created -> C:\ntangle-demo\FooBar\FooBar.Publisher\Services\Generated\ContactHostedService.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: EntityService_cs.hbs (RootCodeGenerator: Publisher/Services)
  [Files: Unchanged = 0, Updated = 0, Created = 0]
 Template: SchemaCdc_sql.hbs (CdcSchemaCreateCodeGenerator: SidecarDb/Migrations (GenOnce))
    Created -> C:\ntangle-demo\FooBar\FooBar.SidecarDb\Migrations\20241112-000032-01-create-ntangle-schema.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: TableVersionTracking_sql.hbs (RootCodeGenerator: SidecarDb/Migrations (GenOnce))
    Created -> C:\ntangle-demo\FooBar\FooBar.SidecarDb\Migrations\20241112-000033-02-create-ntangle-versiontracking-table.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: TableBatchTracking_sql.hbs (TableCodeGenerator: SidecarDb/Migrations (GenOnce))
    Created -> C:\ntangle-demo\FooBar\FooBar.SidecarDb\Migrations\20241112-000033-03-create-ntangle-contactbatchtracking-table.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SchemaEventOutbox_sql.hbs (OutboxCodeGenerator: SidecarDb/Migrations (GenOnce))
    Created -> C:\ntangle-demo\FooBar\FooBar.SidecarDb\Migrations\20241112-000033-04-create-outbox-eventoutbox-schema.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: TableEventOutbox_sql.hbs (OutboxCodeGenerator: SidecarDb/Migrations (GenOnce))
    Created -> C:\ntangle-demo\FooBar\FooBar.SidecarDb\Migrations\20241112-000033-05-create-outbox-eventoutbox-table.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: TableEventOutboxData_sql.hbs (OutboxCodeGenerator: SidecarDb/Migrations (GenOnce))
    Created -> C:\ntangle-demo\FooBar\FooBar.SidecarDb\Migrations\20241112-000033-06-create-outbox-eventoutboxdata-table.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: TableIdentifierMapping_sql.hbs (IdentifierMappingCodeGenerator: SidecarDb/Migrations (GenOnce))
  [Files: Unchanged = 0, Updated = 0, Created = 0]
 Template: CdcEnable_sql.hbs (CdcEnableCodeGenerator: Database/Migrations)
    Created -> C:\ntangle-demo\FooBar\FooBar.Database\Migrations\CdcEnable.post.deploy.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]

FooBar.CodeGen Complete. [1467ms, Files: Unchanged = 0, Updated = 0, Created = 21, TotalLines = 1221]
```

<br/>

### FooBar.Database

As _DbEx_ is being used, the generated artefacts are included in the project automatically. The _source_ artefacts are as follows.

```
└── Schema
  └── Migrations
    └── CdcEnable.post.deploy.sql
```

The `Program.cs` has a default connection string, this needs to be validated and adjusted to that required to enable access to the required SQL Server instance. Compile the application and execute directly from Visual Studio (after setting the _Application arguments_ to `all` within the `Debug` tab of the `Project Properties`), or using `dotnet run migrate`.

The output from the console application should be similar to the following.

```
╔╦╗┌┐ ╔═╗─┐ ┬  ╔╦╗┌─┐┌┬┐┌─┐┌┐ ┌─┐┌─┐┌─┐  ╔╦╗┌─┐┌─┐┬
 ║║├┴┐║╣ ┌┴┬┘   ║║├─┤ │ ├─┤├┴┐├─┤└─┐├┤    ║ │ ││ ││
═╩╝└─┘╚═╝┴ └─  ═╩╝┴ ┴ ┴ ┴ ┴└─┘┴ ┴└─┘└─┘   ╩ └─┘└─┘┴─┘

FooBar.Database Database Tool. [SQL Server]

Command = Migrate
Provider = SqlServer
SchemaOrder = dbo
OutDir = C:\ntangle-demo\FooBar\FooBar.Database
Parameters:
  DatabaseName = FooBar
  JournalSchema = dbo
  JournalTable = SchemaVersions
Assemblies:
  DbEx, Version=2.7.1.0, Culture=neutral, PublicKeyToken=10b60143e92943c1
  DbEx.SqlServer, Version=2.7.1.0, Culture=neutral, PublicKeyToken=10b60143e92943c1
  FooBar.Database, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null

--------------------------------------------------------------------------------

DATABASE MIGRATE: Migrating the database...
  Probing for embedded resources: FooBar.Database.Migrations.*.sql, DbEx.SqlServer.Migrations.*.sql, DbEx.Migrations.*.sql
  Execute the embedded resources...
    FooBar.Database.Migrations.CdcEnable.post.deploy.sql (RES)

Complete. [359.7084ms]

--------------------------------------------------------------------------------

FooBar.Database Complete. [371.9879ms]
```

<br/>

### FooBar.SidecarDb

As _DbEx_ is being used, the generated artefacts are included in the project automatically. The _sidecar_ artefacts are as follows.

```
└── Migrations
  └── yyyymmdd-hhmmss-01-create-ntangle-schema.sql
  └── yyyymmdd-hhmmss-02-create-ntangle-versiontracking-table.sql
  └── yyyymmdd-hhmmss-03-create-ntangle-contactbatchtracking-table.sql
  └── yyyymmdd-hhmmss-04-create-outbox-eventoutbox-schema.sql
  └── yyyymmdd-hhmmss-05-create-outbox-eventoutbox-table.sql
  └── yyyymmdd-hhmmss-06-create-outbox-eventoutboxdata-table.sql
  └── CdcEnable.post.deploy.sql
└── Schema
  └── NTangle
    └── Stored Procedures
      └── Generated
        └── spContactBatchComplete.sql
        └── spContactBatchExecute.sql
        └── spContactBatchReset.sql
        └── spContactBatchTracking.sql
  └── Outbox
    └── Stored Procedures
      └── Generated
        └── spEventOutboxDequeue.sql
        └── spEventOutboxEnqueue.sql
```

The `Program.cs` has a default connection string, this needs to be validated and adjusted to that required to enable access to the required SQL Server instance. Compile the application and execute directly from Visual Studio (after setting the _Application arguments_ to `all` within the `Debug` tab of the `Project Properties`), or using `dotnet run all`.

The output from the console application should be similar to the following.

```
╔╦╗┌┐ ╔═╗─┐ ┬  ╔╦╗┌─┐┌┬┐┌─┐┌┐ ┌─┐┌─┐┌─┐  ╔╦╗┌─┐┌─┐┬
 ║║├┴┐║╣ ┌┴┬┘   ║║├─┤ │ ├─┤├┴┐├─┤└─┐├┤    ║ │ ││ ││
═╩╝└─┘╚═╝┴ └─  ═╩╝┴ ┴ ┴ ┴ ┴└─┘┴ ┴└─┘└─┘   ╩ └─┘└─┘┴─┘

FooBar.SidecarDb Database Tool. [SQL Server]

Command = All
Provider = SqlServer
SchemaOrder = dbo
OutDir = C:\ntangle-demo\FooBar\FooBar.SidecarDb
Parameters:
  DatabaseName = FooBar_Sidecar
  JournalSchema = dbo
  JournalTable = SchemaVersions
Assemblies:
  DbEx, Version=2.7.1.0, Culture=neutral, PublicKeyToken=10b60143e92943c1
  DbEx.SqlServer, Version=2.7.1.0, Culture=neutral, PublicKeyToken=10b60143e92943c1
  FooBar.SidecarDb, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null

--------------------------------------------------------------------------------

DATABASE CREATE: Checking database existence and creating where not found...
  Create database...
    Database 'FooBar_Sidecar' did not exist and was created.

  Probing for 'post.database.create' embedded resources: FooBar.SidecarDb.Migrations.*.sql, DbEx.SqlServer.Migrations.*.sql, DbEx.Migrations.*.sql
  ** Nothing found. **

Complete. [576.864ms]

--------------------------------------------------------------------------------

DATABASE MIGRATE: Migrating the database...
  Probing for embedded resources: FooBar.SidecarDb.Migrations.*.sql, DbEx.SqlServer.Migrations.*.sql, DbEx.Migrations.*.sql
  Execute the embedded resources...
    *Journal table did not exist within the database and was automatically created.
    FooBar.SidecarDb.Migrations.20241112-000032-01-create-ntangle-schema.sql (RES)
    FooBar.SidecarDb.Migrations.20241112-000033-02-create-ntangle-versiontracking-table.sql (RES)
    FooBar.SidecarDb.Migrations.20241112-000033-03-create-ntangle-contactbatchtracking-table.sql (RES)
    FooBar.SidecarDb.Migrations.20241112-000033-04-create-outbox-eventoutbox-schema.sql (RES)
    FooBar.SidecarDb.Migrations.20241112-000033-05-create-outbox-eventoutbox-table.sql (RES)
    FooBar.SidecarDb.Migrations.20241112-000033-06-create-outbox-eventoutboxdata-table.sql (RES)

Complete. [259.1122ms]

--------------------------------------------------------------------------------

DATABASE SCHEMA: Drops and creates the database objects...
  Probing for files (recursively): C:\ntangle-demo\FooBar\FooBar.SidecarDb\Schema\*\*.sql
  Probing for embedded resources: FooBar.SidecarDb.Schema.*.sql, DbEx.SqlServer.Schema.*.sql, DbEx.Schema.*.sql

  Drop known schema objects...
    ** Note: All schema objects implement replace functionality and therefore there is no need to drop existing. **

  Create (or replace) known schema objects...
    FooBar.SidecarDb.Schema.NTangle.Stored_Procedures.Generated.spContactBatchComplete.sql (FILE) > CREATE OR ALTER PROCEDURE [NTangle].[spContactBatchComplete]
    FooBar.SidecarDb.Schema.NTangle.Stored_Procedures.Generated.spContactBatchExecute.sql (FILE) > CREATE OR ALTER PROCEDURE [NTangle].[spContactBatchExecute]
    FooBar.SidecarDb.Schema.NTangle.Stored_Procedures.Generated.spContactBatchReset.sql (FILE) > CREATE OR ALTER PROCEDURE [NTangle].[spContactBatchReset]
    FooBar.SidecarDb.Schema.NTangle.Stored_Procedures.Generated.spContactBatchTracking.sql (FILE) > CREATE OR ALTER PROCEDURE [NTangle].[spContactBatchTracking]
    FooBar.SidecarDb.Schema.Outbox.Stored_Procedures.Generated.spEventOutboxDequeue.sql (FILE) > CREATE OR ALTER PROCEDURE [Outbox].[spEventOutboxDequeue]
    FooBar.SidecarDb.Schema.Outbox.Stored_Procedures.Generated.spEventOutboxEnqueue.sql (FILE) > CREATE OR ALTER PROCEDURE [Outbox].[spEventOutboxEnqueue]

Complete. [86.1015ms]

--------------------------------------------------------------------------------

DATABASE DATA: Insert or merge the embedded data [yaml|json|sql]...
  Probing for embedded resources: DbEx.Data.*, DbEx.SqlServer.Data.*, FooBar.SidecarDb.Data.*
  ** Nothing found. **

Complete. [8.7158ms]

--------------------------------------------------------------------------------

FooBar.SidecarDb Complete. [951.4284ms]
```

<br/>

### FooBar.Publisher

The `Program.cs` is pre-configured, and the `appsettings.json` has the default connection strings, these need to be validated and adjusted to that required to enable access to the required SQL Server instance. All of the generated C# artefacts should have been automatically included within the .NET project. Compile the application and execute. Leave it running; use `ctrl-c` to stop once the following test has been performed.

For the purposes of demonstration the `EventOutboxHostedService` has been configured to use the `LoggerEventSender`; this would need to be changed to use an appropriate `IEventSender` to send the events to an actual messaging system, e.g. Azure [`ServiceBusSender`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Messaging.Azure/ServiceBus/ServiceBusSender.cs).

Within your favorite database tool make a change to `[Legacy].[Contact]` table, updating the `Name` column in the first row. Within the next 5-10 seconds similar console output to the following should be displayed.

```
info: FooBar.Publisher.Data.ContactOrchestrator[0]
      ContactOrchestrator: Batch '1': 1 entity operations(s) were found. [MaxQuerySize=100, ContinueWithDataLoss=True, CorrelationId=4e5bd316-6c8e-466f-8cd1-90dc1298439f, ExecutionId=534c33af-d522-4d70-af2b-680ef2ad11a4, Elapsed=65.3063ms]
info: FooBar.Publisher.Data.ContactOrchestrator[0]
      ContactOrchestrator: Batch '1': 1 event(s) were published successfully. [Publisher=EventPublisher, CorrelationId=4e5bd316-6c8e-466f-8cd1-90dc1298439f, ExecutionId=534c33af-d522-4d70-af2b-680ef2ad11a4, Elapsed=312.5908ms]
info: FooBar.Publisher.Data.ContactOrchestrator[0]
      ContactOrchestrator: Batch '1': Marked as Completed. [CorrelationId=4e5bd316-6c8e-466f-8cd1-90dc1298439f, ExecutionId=534c33af-d522-4d70-af2b-680ef2ad11a4, Elapsed=19.1083ms]
info: CoreEx.Events.LoggerEventSender[0]
      Event[0].Metadata = {
        "id": "391e96d5-85eb-4e24-8c65-cf8c0a9d4143",
        "subject": "legacy.contact",
        "action": "updated",
        "type": "legacy.contact",
        "source": "/database/cdc/legacy/contact/1",
        "timestamp": "2024-11-12T00:12:09.8691761+00:00",
        "correlationId": "4e5bd316-6c8e-466f-8cd1-90dc1298439f",
        "key": "1",
        "etag": "T8jUuxwOIxqXENSPAHEGwnPsD9kwrtyzdGI/acjRrcI="
      }
      Event[0].Data = {
        "specversion": "1.0",
        "id": "391e96d5-85eb-4e24-8c65-cf8c0a9d4143",
        "time": "2024-11-12T00:12:09.8691761Z",
        "type": "legacy.contact",
        "source": "/database/cdc/legacy/contact/1",
        "subject": "legacy.contact",
        "action": "updated",
        "correlationid": "4e5bd316-6c8e-466f-8cd1-90dc1298439f",
        "etag": "T8jUuxwOIxqXENSPAHEGwnPsD9kwrtyzdGI/acjRrcI=",
        "key": "1",
        "datacontenttype": "application/json",
        "data": {
          "id": 1,
          "name": "Bob",
          "phone": "123",
          "addresses": [
            {
              "id": 11,
              "street1": "1st",
              "street2": "Seattle",
              "type": "H"
            },
            {
              "id": 12,
              "street1": "Main",
              "street2": "Redmond",
              "type": "P"
            }
          ],
          "etag": "T8jUuxwOIxqXENSPAHEGwnPsD9kwrtyzdGI/acjRrcI="
        }
      }
```