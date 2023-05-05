# NTangle.Template

This is the .NET template used to accelerate the creation of an _nTangle_ solution and all projects using `dotnet new`. This leverages the .NET Core [templating](https://docs.microsoft.com/en-au/dotnet/core/tools/custom-templates) functionality.

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

The `dotnet new` command is used to create the initial solution artefacts that will leverage Microsoft SQL Server. When creating the database project type can be specified; either using [`DbEx`](https://github.com/Avanade/dbex) (default) or traditional [`Dacpac`](https://docs.microsoft.com/en-us/sql/relational-databases/data-tier-applications/data-tier-applications).

```
dotnet new ntangle
dotnet new ntangle -db DbEx
dotnet new ntangle -db Dacpac
```

<br/>

### What is created?

The following solution and projects will be created within the root `AppName` folder.

```
└── AppName
  └── AppName.CodeGen     # Database and code generation console
  └── AppName.Database    # Datbase project; e.g. DbEx or SQL Server data-tier application (DAC)
  └── AppName.Publisher   # Runtime orchestration and event publishing console
  └── AppName.sln         # Solution file that references all above projects
```

The solution and projects created contain all the requisite .NET Classes and NuGet references to build an _NTangle_ solution.

**Note:** the solution will **not** initially compile. There are references within the [`AppName.Publisher/Program.cs`](./content/AppName.CodeGen/Program.cs) that do not exist; these need to be created/generated.

The templated structure represents the bare minimum needed to start. Generally, the following projects are maintained in the sequence as follows.

- [`AppName.CodeGen`](#AppName.CodeGen)
- [`AppName.Database`](#AppName.Database)
- [`AppName.Publisher`](#AppName.Publisher)

<br/>

### AppName.CodeGen

The connection string defaulted within the [`Program.cs`](./content/AppName.CodeGen/Program.cs) needs to be validated and adjusted to that required for development purposes. The connection string can also be overridden either by using the default environment variable (e.g. `AppName_ConnectionString` where any `.` characters in the `AppName` are replaced with a corresponding `_`), or as a command line argument.

The command line arguments are as follows; execute `dotnet run -- --help`:

```
AppName.CodeGen Code Generation Tool.

Usage: AppName.CodeGen [options]

Options:
  -?|-h|--help              Show help information.
  -s|--script               Script orchestration file/resource name.
  -c|--config               Configuration data file name.
  -o|--output               Output directory path.
  -a|--assembly             Assembly containing embedded resources (multiple can be specified in probing order).
  -p|--param                Parameter expressed as a 'Name=Value' pair (multiple can be specified).
  -cs|--connection-string   Database connection string.
  -cv|--connection-varname  Database connection string environment variable name.
  -enc|--expect-no-changes  Indicates to expect _no_ changes in the artefact output (e.g. error within build pipeline).
  -sim|--simulation         Indicates whether the code-generation is a simulation (i.e. does not create/update any artefacts).
```

Otherwise, the **key** artefact that is maintained is the [`ntangle.yaml`](./content/AppName.CodeGen/ntangle.yaml) which contains the CDC-related table configuration used to drive the code-generation. The contents are provided as an example only, and generally would be removed and replaced with the _actual_ configuration.

To execute the code-generation use `dotnet run`, or execute the `AppName.CodeGen.exe` directly.

<br/>

### AppName.Database (DbEx)

A console application is generated with a reference to _DbEx_ (please review this [repo](https://github.com/Avanade/dbex) to further understand the database migration deployment capabilities).

The connection string defaulted within the [`Program.cs`](./content/AppName.CodeGen/Program.cs) needs to be validated and adjusted to that required for development purposes. The connection string can be overridden either by using the default environment variable (e.g. `AppName_ConnectionString` where any `.` characters in the `AppName` are replaced with a corresponding `_`), or as a command line argument.

The command line arguments are as follows:

```
AppName.Database Database Tool. [SQL Server]

Usage: AppName.Database [options] <command> <args>

Arguments:
  command                    Database migration command (see https://github.com/Avanade/dbex#commands-functions).
                             Allowed values are: None, Drop, Create, Migrate, CodeGen, Schema, Deploy, Reset, Data, DeployWithData, Database, DropAndDatabase, All, DropAndAll,
                             ResetAndData, ResetAndDatabase, ResetAndAll, Execute, Script.
  args                       Additional arguments; 'Script' arguments (first being the script name) -or- 'Execute' (each a SQL statement to invoke).

Options:
  -?|-h|--help               Show help information.
  -cs|--connection-string    Database connection string.
  -cv|--connection-varname   Database connection string environment variable name.
  -so|--schema-order         Database schema name (multiple can be specified in priority order).
  -o|--output                Output directory path.
  -a|--assembly              Assembly containing embedded resources (multiple can be specified in probing order).
  -p|--param                 Parameter expressed as a 'Name=Value' pair (multiple can be specified).
  -eo|--entry-assembly-only  Use the entry assembly only (ignore all other assemblies).
  --accept-prompts           Accept prompts; command should _not_ stop and wait for user confirmation (DROP or RESET commands).

Script command and argument(s):
  script [default]                  Creates a default (empty) SQL script.
  script alter <Schema> <Table>     Creates a SQL script to perform an ALTER TABLE.
  script cdc <Schema> <Table>       Creates a SQL script to turn on CDC for the specified table.
  script cdcdb                      Creates a SQL script to turn on CDC for the database.
  script create <Schema> <Table>    Creates a SQL script to perform a CREATE TABLE.
  script refdata <Schema> <Table>   Creates a SQL script to perform a CREATE TABLE as reference data.
  script schema <Schema>            Creates a SQL script to perform a CREATE SCHEMA.
```

The required artefacts are generally added to this project using the earlier code-generator (see [`AppName.CodeGen`](#AppName.CodeGen)).

To execute the database migration use `dotnet run all`, or execute the `AppName.Database.exe all` directly.

<br/>

### AppName.Database (Dacpac)

A Microsoft SQL Server data-tier application ([DAC](https://docs.microsoft.com/en-us/sql/relational-databases/data-tier-applications/data-tier-applications)) will have been generated. This project will be empty except for [`Post.Deploy.sql`](./content/AppName.Database/Post.Deploy.sql), this executes `:r .\Generated\CdcEnable.post.deploy.sql` which will be created once code-generation has been executed (where configured to do so).

_Tip:_ If after genneration, any of the generated files are not automatically added to the Visual Studio Project structure, the _Show All Files_ in the _Solution Explorer_ can be used to view, and then added en masse by selecting and using the _Include In Project_ function.

_Tip:_ An error `SQL70001: This statement is not recognized in this context` may appear for file `Generated\CdcEnable.sql`. This error is because the _Build Action_ within the _Properties_ for the file defaults to _Build_, change this value to _None_. See this [article](https://arcanecode.com/2013/03/28/ssdt-error-sql70001-this-statement-is-not-recognized-in-this-context/) for more information.

Once ready, the Microsoft SQL Server data-tier application should be used to [_deploy_](https://docs.microsoft.com/en-us/sql/relational-databases/data-tier-applications/deploy-a-data-tier-application) the latest changes.

<br/>

### AppName.Publisher

This default publisher is a console application whose purpose is to orchestrate the CDC processing and corresponding event processing leveraging all of the generated artefacts, both in the database, and within the publisher itself.

The connection string defaulted within the [`appsettings.json`](./content/AppName.Publisher/appsettings.json) needs to be validated and adjusted to that required for development purposes. The connection string can be overridden either by using the default environment variable (e.g. `AppName_ConnectionStrings__SqlDb` where any `.` characters in the `AppName` will be replaced with a corresponding `_`), or as a command line argument (e.g. `ConnectionStrings:SqlDb="value"`).

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
Script = SqlServerDbEx.yaml
OutDir = C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar
ExpectNoChanges = False
IsSimulation = False
Parameters:
  CreateDatabase = System.Func`2[System.String,CoreEx.Database.IDatabase]
  AppName = FooBar
Assemblies:
  FooBar.CodeGen, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
  NTangle, Version=2.0.0.0, Culture=neutral, PublicKeyToken=10b60143e92943c1
  DbEx.SqlServer, Version=2.3.4.0, Culture=neutral, PublicKeyToken=10b60143e92943c1
  DbEx, Version=2.3.4.0, Culture=neutral, PublicKeyToken=10b60143e92943c1

Scripts:
  Querying database to infer table(s)/column(s) schema...
    Database schema query complete [2064.3337ms]

 Template: SpExecuteBatch_sql.hbs (TableCodeGenerator: Database/Schema/Xxx/Stored Procedures)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Schema\NTangle\Stored Procedures\Generated\spContactBatchExecute.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SpCompleteBatch_sql.hbs (TableCodeGenerator: Database/Schema/Xxx/Stored Procedures)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Schema\NTangle\Stored Procedures\Generated\spContactBatchComplete.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: UdtVersionTracking_sql.hbs (RootCodeGenerator: Database/Schema/Xxx/Types/User-Defined Table Types)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Schema\NTangle\Types\User-Defined Table Types\Generated\udtVersionTrackingList.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SpIdentifierMappingCreate_sql.hbs (IdentifierMappingCodeGenerator: Database/Schema/Xxx/Types/Stored Procedures)
  [Files: Unchanged = 0, Updated = 0, Created = 0]
 Template: UdtIdentifierMapping_sql.hbs (IdentifierMappingCodeGenerator: Database/Schema/Xxx/Types/User-Defined Table Types)
  [Files: Unchanged = 0, Updated = 0, Created = 0]
 Template: SpEventOutboxEnqueue_sql.hbs (OutboxCodeGenerator: Database/Schema/Xxx/Types/Stored Procedures)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Schema\Outbox\Stored Procedures\Generated\spEventOutboxEnqueue.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SpEventOutboxDequeue_sql.hbs (OutboxCodeGenerator: Database/Schema/Xxx/Types/Stored Procedures)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Schema\Outbox\Stored Procedures\Generated\spEventOutboxDequeue.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: UdtEventOutbox_sql.hbs (OutboxCodeGenerator: Database/Schema/Xxx/Types/User-Defined Table Types)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Schema\Outbox\Types\User-Defined Table Types\Generated\udtEventOutboxList.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: VersionTrackingMapper_cs.hbs (RootCodeGenerator: Publisher/Data)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Publisher\Data\Generated\VersionTrackingMapper.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: IdentifierMappingMapper_cs.hbs (RootCodeGenerator: Publisher/Data)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Publisher\Data\Generated\IdentifierMappingMapper.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: Entity_cs.hbs (TableCodeGenerator: Publisher/Entities)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Publisher\Entities\Generated\ContactCdc.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: EntityOrchestrator_cs.hbs (TableCodeGenerator: Publisher/Data)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Publisher\Data\Generated\ContactOrchestrator.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: EventOutboxEnqueue_cs.hbs (OutboxCodeGenerator: Publisher/Events)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Publisher\Data\Generated\EventOutboxEnqueue.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: EventOutboxDequeue_cs.hbs (OutboxCodeGenerator: Publisher/Events)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Publisher\Data\Generated\EventOutboxDequeue.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: IServiceCollectionExtensions_cs.hbs (RootCodeGenerator: Publisher)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Publisher\Generated\IServiceCollectionExtensions.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: EntityHostedService_cs.hbs (RootCodeGenerator: Publisher/Services)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Publisher\Services\Generated\ContactHostedService.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SchemaCdc_sql.hbs (CdcSchemaCreateCodeGenerator: Database/Migrations (GenOnce))
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Migrations\20230505-203956-01-create-ntangle-schema.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: TableVersionTracking_sql.hbs (RootCodeGenerator: Database/Migrations (GenOnce))
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Migrations\20230505-203956-02-create-ntangle-versiontracking-table.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: TableBatchTracking_sql.hbs (TableCodeGenerator: Database/Migrations (GenOnce))
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Migrations\20230505-203956-03-create-ntangle-contactbatchtracking-table.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SchemaEventOutbox_sql.hbs (OutboxCodeGenerator: Database/Migrations (GenOnce))
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Migrations\20230505-203956-04-create-outbox-eventoutbox-schema.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: TableEventOutbox_sql.hbs (OutboxCodeGenerator: Database/Migrations (GenOnce))
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Migrations\20230505-203956-05-create-outbox-eventoutbox-table.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: TableEventOutboxData_sql.hbs (OutboxCodeGenerator: Database/Migrations (GenOnce))
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Migrations\20230505-203956-06-create-outbox-eventoutboxdata-table.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: TableIdentifierMapping_sql.hbs (IdentifierMappingCodeGenerator: Database/Migrations (GenOnce))
  [Files: Unchanged = 0, Updated = 0, Created = 0]
 Template: CdcEnable_sql.hbs (CdcEnableCodeGenerator: Database/Migrations)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Migrations\CdcEnable.post.deploy.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]

FooBar.CodeGen Complete. [2556ms, Files: Unchanged = 0, Updated = 0, Created = 21, TotalLines = 1011]
```

<br/>

### FooBar.Database

As _DbEx_ is being used, the generated artefacts are included in the project automatically. The artefacts are as follows.

```
└── Schema
  └── Migrations
    └── yyyymmdd-hhmmss-01-create-ntangle-schema.sql
    └── yyyymmdd-hhmmss-02-create-ntangle-versiontracking-table.sql
    └── yyyymmdd-hhmmss-03-create-ntangle-contactbatchtracking-table.sql
    └── yyyymmdd-hhmmss-04-create-outbox-eventoutbox-schema.sql
    └── yyyymmdd-hhmmss-05-create-outbox-eventoutbox-table.sql
    └── yyyymmdd-hhmmss-06-create-outbox-eventoutboxdata-table.sql
    └── CdcEnable.post.deploy.sql
  └── NTangle
    └── Stored Procedures
      └── Generated
        └── spContactBatchComplete.sql
        └── spContactBatchExecute.sql
    └── Types
      └── User-Defined Table Types
        └── Generated
          └── udtVersionTrackingList.sql
  └── Outbox
    └── Stored Procedures
      └── Generated
        └── spEventOutboxDequeue.sql
        └── spEventOutboxEnqueue.sql
    └── Types
      └── User-Defined Table Types
        └── Generated
          └── udtEventOutboxList.sql
```

The `Program.cs` has a default connection string, this needs to be validated and adjusted to that required to enable access to the required SQL Server instance. Compile the application and execute directly from Visual Studio (after setting the _Application arguments_ to `all` within the `Debug` tab of the `Project Properties`), or using `dotnet run all`.

The output from the console application should be similar to the following.

```
╔╦╗┌┐ ╔═╗─┐ ┬  ╔╦╗┌─┐┌┬┐┌─┐┌┐ ┌─┐┌─┐┌─┐  ╔╦╗┌─┐┌─┐┬
 ║║├┴┐║╣ ┌┴┬┘   ║║├─┤ │ ├─┤├┴┐├─┤└─┐├┤    ║ │ ││ ││
═╩╝└─┘╚═╝┴ └─  ═╩╝┴ ┴ ┴ ┴ ┴└─┘┴ ┴└─┘└─┘   ╩ └─┘└─┘┴─┘

FooBar.Database Database Tool. [SQL Server]

Command = All
Provider = SqlServer
SchemaOrder = dbo
OutDir = C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database
Parameters:
  DatabaseName = FooBar
  JournalSchema = dbo
  JournalTable = SchemaVersions
Assemblies:
  DbEx, Version=2.3.4.0, Culture=neutral, PublicKeyToken=10b60143e92943c1
  DbEx.SqlServer, Version=2.3.4.0, Culture=neutral, PublicKeyToken=10b60143e92943c1
  FooBar.Database, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null

--------------------------------------------------------------------------------

DATABASE CREATE: Checking database existence and creating where not found...
  Create database...
    Database 'FooBar' already exists and therefore not created.

Complete. [1243.4841ms]

--------------------------------------------------------------------------------

DATABASE MIGRATE: Migrating the database...
  Probing for embedded resources: FooBar.Database.Migrations.*.sql, DbEx.SqlServer.Migrations.*.sql, DbEx.Migrations.*.sql
  Execute the embedded resources...
    *Journal table did not exist within the database and was automatically created.
    FooBar.Database.Migrations.20230505-203956-01-create-ntangle-schema.sql
    FooBar.Database.Migrations.20230505-203956-02-create-ntangle-versiontracking-table.sql
    FooBar.Database.Migrations.20230505-203956-03-create-ntangle-contactbatchtracking-table.sql
    FooBar.Database.Migrations.20230505-203956-04-create-outbox-eventoutbox-schema.sql
    FooBar.Database.Migrations.20230505-203956-05-create-outbox-eventoutbox-table.sql
    FooBar.Database.Migrations.20230505-203956-06-create-outbox-eventoutboxdata-table.sql
    FooBar.Database.Migrations.CdcEnable.post.deploy.sql

Complete. [8585.2454ms]

--------------------------------------------------------------------------------

DATABASE SCHEMA: Drops and creates the database objects...
  Probing for files (recursively): C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Schema\*\*.sql
  Probing for embedded resources: FooBar.Database.Schema.*.sql, DbEx.SqlServer.Schema.*.sql, DbEx.Schema.*.sql

  Drop known schema objects...
    DROP PROCEDURE IF EXISTS [Outbox].[spEventOutboxEnqueue]
    DROP PROCEDURE IF EXISTS [Outbox].[spEventOutboxDequeue]
    DROP PROCEDURE IF EXISTS [NTangle].[spContactBatchExecute]
    DROP PROCEDURE IF EXISTS [NTangle].[spContactBatchComplete]
    DROP TYPE IF EXISTS [Outbox].[udtEventOutboxList]
    DROP TYPE IF EXISTS [NTangle].[udtVersionTrackingList]

  Create known schema objects...
    FooBar.Database.Schema.NTangle.Types.User_Defined_Table_Types.Generated.udtVersionTrackingList.sql > CREATE TYPE [NTangle].[udtVersionTrackingList]
    FooBar.Database.Schema.Outbox.Types.User_Defined_Table_Types.Generated.udtEventOutboxList.sql > CREATE TYPE [Outbox].[udtEventOutboxList]
    FooBar.Database.Schema.NTangle.Stored_Procedures.Generated.spContactBatchComplete.sql > CREATE PROCEDURE [NTangle].[spContactBatchComplete]
    FooBar.Database.Schema.NTangle.Stored_Procedures.Generated.spContactBatchExecute.sql > CREATE PROCEDURE [NTangle].[spContactBatchExecute]
    FooBar.Database.Schema.Outbox.Stored_Procedures.Generated.spEventOutboxDequeue.sql > CREATE PROCEDURE [Outbox].[spEventOutboxDequeue]
    FooBar.Database.Schema.Outbox.Stored_Procedures.Generated.spEventOutboxEnqueue.sql > CREATE PROCEDURE [Outbox].[spEventOutboxEnqueue]

Complete. [422.1665ms]

--------------------------------------------------------------------------------

DATABASE DATA: Insert or merge the embedded YAML data...
  Probing for embedded resources: DbEx.Data.*.[sql|yaml], DbEx.SqlServer.Data.*.[sql|yaml], FooBar.Database.Data.*.[sql|yaml]
  ** Nothing found. **

Complete. [7.6648ms]

--------------------------------------------------------------------------------

FooBar.Database Complete. [10271.7272ms]
```

<br/>

### FooBar.Publisher

The `Program.cs` is pre-configured, and the `appsettings.json` has a default connection string, this needs to be validated and adjusted to that required to enable access to the required SQL Server instance. All of the generated C# artefacts should have been automatically included within the .NET project. Compile the application and execute. Leave it running; use `ctrl-c` to stop once the following test has been performed.

For the purposes of demonstration the `EventOutboxHostedService` has been configured to use the `LoggerEventSender`; this would need to be changed to use an appropriate `IEventSender` to send the events to an actual messaging system, e.g. Azure [`ServiceBusSender`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Messaging.Azure/ServiceBus/ServiceBusSender.cs).

Within your favorite database tool make a change to `[Legacy].[Contact]` table, updating the `Name` column in the first row. Within the next 5-10 seconds similar console output to the following should be displayed.

```
info: FooBar.Publisher.Data.ContactOrchestrator[0]
      Batch '1': 1 entity operations(s) were found. [MaxQuerySize=100, ContinueWithDataLoss=True, CorrelationId=27f19f01-aee7-4df7-b318-356960480a7d, ExecutionId=c553797c-77b2-4818-892a-80a126b63546, Elapsed=143.5141ms]
info: FooBar.Publisher.Data.ContactOrchestrator[0]
      Batch '1': 1 event(s) were published successfully. [Publisher=EventPublisher, CorrelationId=27f19f01-aee7-4df7-b318-356960480a7d, ExecutionId=c553797c-77b2-4818-892a-80a126b63546, Elapsed=558.2567ms]
info: FooBar.Publisher.Data.ContactOrchestrator[0]
      Batch '1': Marked as Completed. [CorrelationId=27f19f01-aee7-4df7-b318-356960480a7d, ExecutionId=c553797c-77b2-4818-892a-80a126b63546, Elapsed=27.2628ms]
info: FooBar.Publisher.Data.EventOutboxDequeue[0]
      1 event(s) were dequeued. [Elapsed=54.2871ms]
info: CoreEx.Events.LoggerEventSender[0]
      Event[0].Metadata = {
        "id": "9cd0e900-4679-4fb0-8e79-9e7c1b0d144e",
        "subject": "legacy.contact",
        "action": "updated",
        "type": "legacy.contact",
        "source": "/database/cdc/legacy/contact/1",
        "timestamp": "2023-05-05T20:46:27.0571826+00:00",
        "correlationId": "27f19f01-aee7-4df7-b318-356960480a7d",
        "key": "1",
        "etag": "RR/C\u002Bdq/uV0ipsTJNzhtyioHDhpA3ko4ZnXmvmsMhsE="
      }
      Event[0].Data = {
        "specversion": "1.0",
        "id": "9cd0e900-4679-4fb0-8e79-9e7c1b0d144e",
        "time": "2023-05-05T20:46:27.0571826Z",
        "type": "legacy.contact",
        "source": "/database/cdc/legacy/contact/1",
        "subject": "legacy.contact",
        "action": "updated",
        "correlationid": "27f19f01-aee7-4df7-b318-356960480a7d",
        "etag": "RR/C\u002Bdq/uV0ipsTJNzhtyioHDhpA3ko4ZnXmvmsMhsE=",
        "datacontenttype": "application/json",
        "data": {
          "id": 1,
          "name": "Bobby",
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
          "etag": "RR/C\u002Bdq/uV0ipsTJNzhtyioHDhpA3ko4ZnXmvmsMhsE="
        }
      }
info: FooBar.Publisher.Data.EventOutboxDequeue[0]
      1 event(s) were sent successfully. [Sender=LoggerEventSender, Elapsed=135.5593ms]
```
