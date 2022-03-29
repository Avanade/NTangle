# NTangle.Template

This is the .NET template used to accelerate the creation of an _nTangle_ solution and all projects using `dotnet new`. This leverages the .NET Core [templating](https://docs.microsoft.com/en-au/dotnet/core/tools/custom-templates) functionality.

<br/>

## Installation

Before the `NTangle.Template` template can be used it must be installed from [NuGet](https://www.nuget.org/packages/NTangle.Template/). The `dotnet new -i` command is used to perform this.

```
-- Use the latest published from NuGet...
dotnet new -i ntangle.template --nuget-source https://api.nuget.org/v3/index.json

-- Or alternatively, point to a local folder...
dotnet new -i ntangle.template --nuget-source C:\Users\Name\nuget-publish
```

To verify once installed, execute `dotnet new ntangle --help`. This will output the following showing the supported _NTangle_ command line options.

```
NTangle solution (C#)
Author: NTangle developers
Options:
  -db|--dbproject  The database project option.
                       DbEx      - Indicates to use DbEx-based deployment migration application.
                       Dacpac    - Indicates to use SQL Server data-tier application (DACPAC).
                       None      - Indicates that a database project is not required.
                   Default: DbEx
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
  └── AppName.Database    # Datbase project; e.g. SQL Server data-tier application (DAC)
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

The connection string defaulted within the [`Program.cs`](./content/AppName.CodeGen/Program.cs) needs to be validated and adjusted to that required for development purposes. The connection string can be overridden either by using the default environment variable (e.g. `AppName_ConnectionString` where any `.` characters in the `AppName` are replaced with a corresponding `_`), or as a command line argument.

The command line arguments are as follows:

```
AppName.CodeGen

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

Otherwise, the **key** artefact that is maintained is [`ntangle.yaml`](./content/AppName.CodeGen/ntangle.yaml) that contains the CDC-related table configuration used to drive the code-generation. The contents are provided as an example only, and generally would be removed and replaced with the _actual_ configuration.

To execute the code-generation use `dotnet run`, or execute the `AppName.CodeGen.exe` directly.

<br/>

### AppName.Database (DbEx)

A console application is generated with a reference to _DbEx_ (please review this [repo](https://github.com/Avanade/dbex) to further understand the database migration deployment capabilities).

The connection string defaulted within the [`Program.cs`](./content/AppName.CodeGen/Program.cs) needs to be validated and adjusted to that required for development purposes. The connection string can be overridden either by using the default environment variable (e.g. `AppName_ConnectionString` where any `.` characters in the `AppName` are replaced with a corresponding `_`), or as a command line argument.

The command line arguments are as follows:

```
AppName.Database

Usage: AppName.Database [options] <command> <script-args>

Arguments:
  command                    Database migration command.
                             Allowed values are: None, Drop, Create, Migrate, Schema, Reset, Data, All, DropAndAll, ResetAndData, ResetAndAll, Script.
  script-args                Arguments for the Script command (first being the script name).

Options:
  -?|-h|--help               Show help information.
  -cs|--connection-string    Database connection string.
  -cv|--connection-varname   Database connection string environment variable name.
  -so|--schema-order         Database schema name (multiple can be specified in priority order).
  -o|--output                Output directory path.
  -a|--assembly              Assembly containing embedded resources (multiple can be specified in probing order).
  -eo|--entry-assembly-only  Use the entry assembly only (ignore all other assemblies).
```

All required artefacts are generally added to this project using [`AppName.CodeGen`](#AppName.CodeGen).

To execute the database migration use `dotnet run`, or execute the `AppName.Database.exe` directly.

<br/>

### AppName.Database (Dacpac)

A Microsoft SQL Server data-tier application ([DAC](https://docs.microsoft.com/en-us/sql/relational-databases/data-tier-applications/data-tier-applications)) will have been generated. This project will be empty except for [`Post.Deploy.sql`](./content/AppName.Database/Post.Deploy.sql), this executes `:r .\Generated\CdcEnable.post.deploy.sql` which will be created once code-generation has been executed (where configured to do so, which is the default).

_Tip:_ If after genneration, any of the generated files are not automatically added to the Visual Studio Project structure, the _Show All Files_ in the _Solution Explorer_ can be used to view, and then added en masse by selecting and using the _Include In Project_ function.

_Tip:_ An error `SQL70001: This statement is not recognized in this context` may appear for file `Generated\CdcEnable.sql`. This error is because the _Build Action_ within the _Properties_ for the file defaults to _Build_, change this value to _None_. See this [article](https://arcanecode.com/2013/03/28/ssdt-error-sql70001-this-statement-is-not-recognized-in-this-context/) for more information.

Once ready, the Microsoft SQL Server data-tier application should be used to [_deploy_](https://docs.microsoft.com/en-us/sql/relational-databases/data-tier-applications/deploy-a-data-tier-application) the latest changes.

<br/>

### AppName.Publisher

This default publisher is a console application whose purpose is to orchestrate the CDC processing and corresponding event processing leveraging all of the generated artefacts, both in the database, and within the publisher itself.

The connection string defaulted within the [`appsettings.json`](./content/AppName.Publisher/appsettings.json) needs to be validated and adjusted to that required for development purposes. The connection string can be overridden either by using the default environment variable (e.g. `AppName_ConnectionStrings__SqlDb` where any `.` characters in the `AppName` will be replaced with a corresponding `_`), or as a command line argument (e.g. `ConnectionStrings:SqlDb="value"`).

Additionally, the following configuration settings can be used to control execution.

```
// Orchestration hosted service(s) configurations:
Interval=00:00:05                       // Sets the interval timespan between orchestrations for all aggregate entities. 
Entity_Interval=00:00:05                // Sets the interval timespan between orchestrations for the specified entity; e.g. Contact_Interval=00:00:05
MaxQuerySize=50                         // Sets the maximum query size for all aggregate entities.
Entity_MaxQuerySize=50                  // Sets the maximum query size for the specified entity; e.g. Contact_MaxQuerySize=50
ContinueWithDataLoss=true               // Indicates to continue with potential data loss for all aggregate entities. 
Entity_ContinueWithDataLoss=true        // Indicates to continue with potential data loss for for the specified entity; e.g. Contact_ContinueWithDataLoss=true
Services=Entity1,Entity2,EntityN        // Comma-separated list of entities to be orchestrated; default is all; e.g. Services=Contact,Person

// Outbox dequeue and publish hosted service configuration:
OutboxInterval=00:00:05                 // Sets the interval timespan for the outbox dequeue and publishing.
OutboxMaxDequeueSize=100                // Sets the maximum outbox dequeue size (number of events) per execution.
```

<br/>

## Demonstration

The following walks through the process of demonstrating how to execute the end-to-end functionality of _NTangle_. For the purposes of the demo the application name will be `FooBar`.

<br/>

### Create database

An existing database is required that contains tables and data. To create the database and set up, copy the contents of [`create-database.sql`](./create-database.sql) and execute using your favorite database tool.

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

The `ntangle.yaml` configuration file is pre-configured, and `Program.cs` has the correct connection string. Compile the application and execute directly from Visual Studio, or using `dotnet run`.

The output from the console application should be similar to the following.

```
╔╗╔╔╦╗┌─┐┌┐┌┌─┐┬  ┌─┐  ╔═╗┌─┐┌┬┐┌─┐  ╔═╗┌─┐┌┐┌  ╔╦╗┌─┐┌─┐┬  
║║║ ║ ├─┤││││ ┬│  ├┤   ║  │ │ ││├┤───║ ╦├┤ │││   ║ │ ││ ││  
╝╚╝ ╩ ┴ ┴┘└┘└─┘┴─┘└─┘  ╚═╝└─┘─┴┘└─┘  ╚═╝└─┘┘└┘   ╩ └─┘└─┘┴─┘

FooBar.CodeGen Code Generation Tool.

Config = ntangle.yaml
Script = SqlServerDbEx.yaml
OutDir = C:\Users\eric\source\repos\FooBar
ExpectNoChanges = False
IsSimulation = False
Parameters:
  CreateDatabase = System.Func`2[System.String,NTangle.Data.SqlServer.SqlServerDatabase]
  DbProvider = SqlServer
  AppName = FooBar
Assemblies:
  FooBar.CodeGen, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
  NTangle, Version=1.0.3.0, Culture=neutral, PublicKeyToken=10b60143e92943c1
  DbEx, Version=1.0.4.0, Culture=neutral, PublicKeyToken=10b60143e92943c1

Scripts:
  Querying database to infer table(s)/column(s) schema...
    Database schema query complete [1723ms]

 Template: SqlServer/SpExecuteBatch_sql.hbs (TableCodeGenerator: Database/Schema/Xxx/Stored Procedures)
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Database\Schema\NTangle\Stored Procedures\Generated\spContactBatchExecute.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/SpCompleteBatch_sql.hbs (TableCodeGenerator: Database/Schema/Xxx/Stored Procedures)
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Database\Schema\NTangle\Stored Procedures\Generated\spContactBatchComplete.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/UdtVersionTracking_sql.hbs (RootCodeGenerator: Database/Schema/Xxx/Types/User-Defined Table Types)
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Database\Schema\NTangle\Types\User-Defined Table Types\Generated\udtVersionTrackingList.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/SpIdentifierMappingCreate_sql.hbs (IdentifierMappingCodeGenerator: Database/Schema/Xxx/Types/Stored Procedures)
  [Files: Unchanged = 0, Updated = 0, Created = 0]
 Template: SqlServer/UdtIdentifierMapping_sql.hbs (IdentifierMappingCodeGenerator: Database/Schema/Xxx/Types/User-Defined Table Types)
  [Files: Unchanged = 0, Updated = 0, Created = 0]
 Template: SqlServer/SpEventOutboxEnqueue_sql.hbs (OutboxCodeGenerator: Database/Schema/Xxx/Types/Stored Procedures)
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Database\Schema\Outbox\Stored Procedures\Generated\spEventOutboxEnqueue.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/SpEventOutboxDequeue_sql.hbs (OutboxCodeGenerator: Database/Schema/Xxx/Types/Stored Procedures)
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Database\Schema\Outbox\Stored Procedures\Generated\spEventOutboxDequeue.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/UdtEventOutbox_sql.hbs (OutboxCodeGenerator: Database/Schema/Xxx/Types/User-Defined Table Types)
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Database\Schema\Outbox\Types\User-Defined Table Types\Generated\udtEventOutboxList.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/VersionTrackingMapper_cs.hbs (RootCodeGenerator: Publisher/Data)
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Publisher\Data\Generated\VersionTrackingMapper.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/IdentifierMappingMapper_cs.hbs (RootCodeGenerator: Publisher/Data)
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Publisher\Data\Generated\IdentifierMappingMapper.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: Entity_cs.hbs (TableCodeGenerator: Publisher/Entities)
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Publisher\Entities\Generated\ContactCdc.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: EntityOrchestrator_cs.hbs (TableCodeGenerator: Publisher/Data)
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Publisher\Data\Generated\ContactCdcOrchestrator.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/EventOutboxEnqueue_cs.hbs (OutboxCodeGenerator: Publisher/Events)
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Publisher\Data\Generated\EventOutboxEnqueue.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/EventOutboxDequeue_cs.hbs (OutboxCodeGenerator: Publisher/Events)
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Publisher\Data\Generated\EventOutboxDequeue.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: IServiceCollectionExtensions_cs.hbs (RootCodeGenerator: Publisher)
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Publisher\Generated\IServiceCollectionExtensions.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: EntityHostedService_cs.hbs (RootCodeGenerator: Publisher/Services)
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Publisher\Services\Generated\ContactHostedService.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/SchemaCdc_sql.hbs (CdcSchemaCreateCodeGenerator: Database/Migrations (GenOnce))
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Database\Migrations\20220328-201828-01-create-ntangle-schema.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/TableVersionTracking_sql.hbs (RootCodeGenerator: Database/Migrations (GenOnce))
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Database\Migrations\20220328-201828-02-create-ntangle-versiontracking-table.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/TableBatchTracking_sql.hbs (TableCodeGenerator: Database/Migrations (GenOnce))
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Database\Migrations\20220328-201828-03-create-ntangle-contactbatchtracking-table.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/SchemaEventOutbox_sql.hbs (OutboxCodeGenerator: Database/Migrations (GenOnce))
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Database\Migrations\20220328-201828-04-create-outbox-eventoutbox-schema.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/TableEventOutbox_sql.hbs (OutboxCodeGenerator: Database/Migrations (GenOnce))
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Database\Migrations\20220328-201828-05-create-outbox-eventoutbox-table.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/TableEventOutboxData_sql.hbs (OutboxCodeGenerator: Database/Migrations (GenOnce))
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Database\Migrations\20220328-201828-06-create-outbox-eventoutboxdata-table.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/TableIdentifierMapping_sql.hbs (IdentifierMappingCodeGenerator: Database/Migrations (GenOnce))
  [Files: Unchanged = 0, Updated = 0, Created = 0]
 Template: SqlServer/CdcEnable_sql.hbs (CdcEnableCodeGenerator: Database/Migrations)
    Created -> C:\Users\eric\source\repos\FooBar\FooBar.Database\Migrations\CdcEnable.post.deploy.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]

FooBar.CodeGen Complete. [1347ms, Files: Unchanged = 0, Updated = 0, Created = 21, TotalLines = 1026]
```

<br/>

### FooBar.Database

As _DbEx_ is being used, the generated artefacts are included in the project automatically. The artefacts are as follows.

```
└── Generated
  └── CdcEnable.sql
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

The `Program.cs` has the correct connection string. Compile the application and execute directly from Visual Studio (after setting the _Application arguments_ to `all` within the `Debug` tab of the `Project Properties`), or using `dotnet run all`.

The output from the console application should be similar to the following.

```
╔╦╗┌┐ ╔═╗─┐ ┬  ╔╦╗┌─┐┌┬┐┌─┐┌┐ ┌─┐┌─┐┌─┐  ╔╦╗┌─┐┌─┐┬  
 ║║├┴┐║╣ ┌┴┬┘   ║║├─┤ │ ├─┤├┴┐├─┤└─┐├┤    ║ │ ││ ││  
═╩╝└─┘╚═╝┴ └─  ═╩╝┴ ┴ ┴ ┴ ┴└─┘┴ ┴└─┘└─┘   ╩ └─┘└─┘┴─┘

FooBar.Database Database Tool.

Command = Deploy
SchemaOrder = 
OutDir = 
Assemblies:
  FooBar.Database, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
  DbEx, Version=1.0.4.0, Culture=neutral, PublicKeyToken=10b60143e92943c1

--------------------------------------------------------------------------------

DATABASE CREATE: Checking database existence and creating where not found...
  Create database (using DbUp)...
    Master ConnectionString => Data Source=.;Initial Catalog=master;Integrated Security=True;Password=

Complete [400ms].

--------------------------------------------------------------------------------

DATABASE MIGRATE: Migrating the database...
  Probing for embedded resources: FooBar.Database.Migrations.*.sql, DbEx.Migrations.*.sql
  Migrate (using DbUp) the embedded resources...
    Beginning database upgrade
    Checking whether journal table exists..
    Fetching list of already executed scripts.
    Executing Database Server script 'FooBar.Database.Migrations.20220328-201828-01-create-ntangle-schema.sql'
    Checking whether journal table exists..
    Executing Database Server script 'FooBar.Database.Migrations.20220328-201828-02-create-ntangle-versiontracking-table.sql'
    Executing Database Server script 'FooBar.Database.Migrations.20220328-201828-03-create-ntangle-contactbatchtracking-table.sql'
    Executing Database Server script 'FooBar.Database.Migrations.20220328-201828-04-create-outbox-eventoutbox-schema.sql'
    Executing Database Server script 'FooBar.Database.Migrations.20220328-201828-05-create-outbox-eventoutbox-table.sql'
    Executing Database Server script 'FooBar.Database.Migrations.20220328-201828-06-create-outbox-eventoutboxdata-table.sql'
    Executing Database Server script 'FooBar.Database.Migrations.CdcEnable.post.deploy.sql'
    Upgrade successful

Complete. [8917ms]

--------------------------------------------------------------------------------

DATABASE SCHEMA: Drops and creates the database objects...
  Probing for files (recursively): C:\Users\eric\source\repos\FooBar\FooBar.Database\Schema\*\*.sql
  Probing for embedded resources: FooBar.Database.Schema.*.sql, DbEx.Schema.*.sql
  Drop (using DbUp) known schema objects...
    Beginning database upgrade
    Checking whether journal table exists..
    Fetching list of already executed scripts.
    Executing Database Server script 'DROP PROCEDURE IF EXISTS [Outbox].[spEventOutboxEnqueue]'
    Checking whether journal table exists..
    Executing Database Server script 'DROP PROCEDURE IF EXISTS [Outbox].[spEventOutboxDequeue]'
    Executing Database Server script 'DROP PROCEDURE IF EXISTS [NTangle].[spContactBatchExecute]'
    Executing Database Server script 'DROP PROCEDURE IF EXISTS [NTangle].[spContactBatchComplete]'
    Executing Database Server script 'DROP TYPE IF EXISTS [NTangle].[udtVersionTrackingList]'
    Executing Database Server script 'DROP TYPE IF EXISTS [Outbox].[udtEventOutboxList]'
    Upgrade successful
  Create (using DbUp) known schema objects...
    Beginning database upgrade
    Checking whether journal table exists..
    Fetching list of already executed scripts.
    Executing Database Server script 'FooBar.Database.Schema.Outbox.Types.User_Defined_Table_Types.Generated.udtEventOutboxList.sql'
    Checking whether journal table exists..
    Executing Database Server script 'FooBar.Database.Schema.NTangle.Types.User_Defined_Table_Types.Generated.udtVersionTrackingList.sql'
    Executing Database Server script 'FooBar.Database.Schema.NTangle.Stored_Procedures.Generated.spContactBatchComplete.sql'
    Executing Database Server script 'FooBar.Database.Schema.NTangle.Stored_Procedures.Generated.spContactBatchExecute.sql'
    Executing Database Server script 'FooBar.Database.Schema.Outbox.Stored_Procedures.Generated.spEventOutboxDequeue.sql'
    Executing Database Server script 'FooBar.Database.Schema.Outbox.Stored_Procedures.Generated.spEventOutboxEnqueue.sql'
    Upgrade successful

Complete. [742ms]

--------------------------------------------------------------------------------

DATABASE DATA: Insert or merge the embedded YAML data...
  Probing for embedded resources: DbEx.Data.*.sql, FooBar.Database.Data.*.sql
  ** Nothing found. **

Complete [6ms].

--------------------------------------------------------------------------------

FooBar.Database Complete. [8063ms]
```

<br/>

### FooBar.Publisher

The `Program.cs` is pre-configured, and the `appsettings.json` has the correct connection string. All of the generated C# artefacts should have been automatically included within the .NET project. Compile the application and execute. Leave it running; use `ctrl-c` to stop once the following test has been performed.

For the purposes of demonstration the final/primary `IEventSender` has been configured to use `LoggerEventSender`; this would be changed to use an appropriate `IEventSender` to send the events to an actual messaging system, e.g. Azure [`ServiceBusSender`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Messaging.Azure/ServiceBus/ServiceBusSender.cs).

Within your favorite database tool make a change to `[Legacy].[Contact]` table, updating the `Name` column in the first row. Within the next 30 seconds similar console output to the following should be displayed.

```
info: FooBar.Publisher.Services.ContactHostedService[0]
      ContactHostedService started. Timer first/interval 00:00:01/00:00:01.
info: NTangle.Services.EventOutboxHostedService[0]
      EventOutboxHostedService started. Timer first/interval 00:00:02/00:00:02.
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: C:\Users\eric\source\repos\FooBar\FooBar.Publisher
info: FooBar.Publisher.Data.ContactCdcOrchestrator[0]
      Batch '1': 1 entity operations(s) were found. [MaxQuerySize=50, ContinueWithDataLoss=True, CorrelationId=c2b99065-1dec-423f-b01a-6d6257014058, ExecutionId=2f4d422d-2371-4181-ba88-5d9f5975b94f, Elapsed=1308ms]
info: CoreEx.Events.LoggerEventSender[0]
      Event[0].Data = 
      {
        "specversion": "1.0",
        "id": "ca85a3f3-501b-4404-9e50-cb25f50605a2",
        "time": "2022-03-29T14:56:10.5255554Z",
        "type": "legacy.contact.updated",
        "source": "/database/cdc/legacy/contact/1",
        "subject": "legacy.contact",
        "action": "updated",
        "correlationid": "c2b99065-1dec-423f-b01a-6d6257014058",
        "datacontenttype": "application/json",
        "data": {
          "id": 1,
          "name": "Bob",
          "phone": "000",
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
          "etag": "u7OJKfm6B2jwJfg3o1/Ea7bTmuSbnXwTCme3z0FjQmw="
        }
      }
info: FooBar.Publisher.Data.ContactCdcOrchestrator[0]
      Batch '1': 1 event(s) were published successfully. [Publisher=EventPublisher, CorrelationId=c2b99065-1dec-423f-b01a-6d6257014058, ExecutionId=2f4d422d-2371-4181-ba88-5d9f5975b94f, Elapsed=340ms]
info: FooBar.Publisher.Data.ContactCdcOrchestrator[0]
      Batch '1': Marked as Completed. [CorrelationId=c2b99065-1dec-423f-b01a-6d6257014058, ExecutionId=2f4d422d-2371-4181-ba88-5d9f5975b94f, Elapsed=23ms]
```