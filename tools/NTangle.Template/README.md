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

<br/>

## Create solution

To create the _Solution_ you must first be in the directory that you intend to create the artefacts within. The directory name is then used as the default for the application name.

The `dotnet new` command is used to create the initial solution artefacts that will leverage Microsoft SQL Server, and by extension a Microsoft SQL Server data-tier application, by default.

```
dotnet new ntangle
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

The connection string defaulted within the [`Program.cs`](./content/AppName.CodeGen/Program.cs) needs to be validated and adjusted to that required for development purposes. The connection string can be overridden either by using the default environment variable (e.g. `AppName_ConnectionString` where any `.` characters in the `AppName` will be replaced with a corresponding `_`), or as a command line argument.

The command line arguments are as follows:

```
NTangle Code Generator.

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

To execute the code-generation use `dotnet run` or execute the `AppName.CodeGen.exe` directly.

<br/>

### AppName.Database

A Microsoft SQL Server data-tier application ([DAC](https://docs.microsoft.com/en-us/sql/relational-databases/data-tier-applications/data-tier-applications)) will have been generated. This project will be empty except for [`Post.Deploy.sql`](./content/AppName.Database/Post.Deploy.sql), this executes `:r .\Generated\CdcEnable.sql` which will be created once code-generation has been executed (where configured to do so, which is the default).

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
Entity_ContinueWithDataLoss=true        // Indicates to continue with potential data loss for all aggregate entities.
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

_Tip:_ Where attempting for a subsequent time and the database already exists use the following to drop prior to recreating a fresh version.

```
USE master
ALTER DATABASE [FooBar] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
DROP DATABASE [FooBar]
```

<br/>

### Create solution

To start, create a new `FooBar` directory, change to that directory, and then create the solution using the _NTangle_ template. Once created, open the solution in Visual Studio.

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

NTangle [v1.0.1]

Config = ntangle.yaml
Script = SqlServerDacpac.yaml
OutDir = C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar
ExpectNoChanges = False
IsSimulation = False
Parameters:
  AppName = FooBar
  DbProvider = SqlServer
Assemblies:
  FooBar.CodeGen, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
  NTangle, Version=1.0.1.0, Culture=neutral, PublicKeyToken=10b60143e92943c1

Scripts:
  Querying database to infer table(s)/column(s) schema...
    Database query complete [1149ms]

 Template: SqlServer/SpExecuteBatch_sql.hb (TableCodeGenerator: Database/Schema/Xxx/Stored Procedures)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Schema\NTangle\Stored Procedures\Generated\spContactBatchExecute.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/SpCompleteBatch_sql.hb (TableCodeGenerator: Database/Schema/Xxx/Stored Procedures)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Schema\NTangle\Stored Procedures\Generated\spContactBatchComplete.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/UdtVersionTracking_sql.hb (RootCodeGenerator: Database/Schema/Xxx/Types/User-Defined Table Types)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Schema\NTangle\Types\User-Defined Table Types\Generated\udtVersionTrackingList.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/SpIdentifierMappingCreate_sql.hb (IdentifierMappingCodeGenerator: Database/Schema/Xxx/Types/Stored Procedures)
  [Files: Unchanged = 0, Updated = 0, Created = 0]
 Template: SqlServer/UdtIdentifierMapping_sql.hb (IdentifierMappingCodeGenerator: Database/Schema/Xxx/Types/User-Defined Table Types)
  [Files: Unchanged = 0, Updated = 0, Created = 0]
 Template: SqlServer/SpOutboxEnqueue_sql.hb (OutboxCodeGenerator: Database/Schema/Xxx/Types/Stored Procedures)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Schema\NTangle\Stored Procedures\Generated\spEventOutboxEnqueue.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/SpOutboxDequeue_sql.hb (OutboxCodeGenerator: Database/Schema/Xxx/Types/Stored Procedures)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Schema\NTangle\Stored Procedures\Generated\spEventOutboxDequeue.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/UdtEventOutbox_sql.hb (OutboxCodeGenerator: Database/Schema/Xxx/Types/User-Defined Table Types)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Schema\NTangle\Types\User-Defined Table Types\Generated\udtEventOutboxList.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/VersionTrackingMapper_cs.hb (RootCodeGenerator: Publisher/Data)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Publisher\Data\Generated\VersionTrackingMapper.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/IdentifierMappingMapper_cs.hb (RootCodeGenerator: Publisher/Data)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Publisher\Data\Generated\IdentifierMappingMapper.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/EventOutboxMapper_cs.hb (RootCodeGenerator: Publisher/Data)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Publisher\Data\Generated\EventOutboxMapper.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: Entity_cs.hb (TableCodeGenerator: Publisher/Entities)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Publisher\Entities\Generated\ContactCdc.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: EntityOrchestrator_cs.hb (TableCodeGenerator: Publisher/Data)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Publisher\Data\Generated\ContactCdcOrchestrator.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: OutboxEventPublisher_cs.hb (OutboxCodeGenerator: Publisher/Events)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Publisher\Events\Generated\OutboxEventPublisher.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: ServiceCollectionExtensions_cs.hb (RootCodeGenerator: Publisher)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Publisher\Generated\ServiceCollectionExtensions.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: EntityHostedService_cs.hb (RootCodeGenerator: Publisher/Services)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Publisher\Services\Generated\ContactHostedService.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: OutboxDequeueHostedService_cs.hb (OutboxCodeGenerator: Publisher/Services)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Publisher\Services\Generated\OutboxDequeueHostedService.cs
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/CdcEnable_sql.hb (CdcEnableCodeGenerator: Database)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Generated\CdcEnable.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/SchemaCdc_sql.hb (CdcSchemaCreateCodeGenerator: Database/Schema/Xxx)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Schema\NTangle\Generated\NTangle.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/TableVersionTracking_sql.hb (RootCodeGenerator: Database/Schema/Xxx/Tables)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Schema\NTangle\Tables\Generated\VersionTracking.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/TableBatchTracking_sql.hb (TableCodeGenerator: Database/Schema/Xxx/Tables)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Schema\NTangle\Tables\Generated\ContactBatchTracking.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/TableIdentifierMapping_sql.hb (IdentifierMappingCodeGenerator: Database/Schema/Xxx/Tables)
  [Files: Unchanged = 0, Updated = 0, Created = 0]
 Template: SqlServer/TableEventOutbox_sql.hb (OutboxCodeGenerator: Database/Schema/Xxx/Tables)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Schema\NTangle\Tables\Generated\EventOutbox.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]
 Template: SqlServer/TableEventOutboxData_sql.hb (OutboxCodeGenerator: Database/Schema/Xxx/Tables)
    Created -> C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Database\Schema\NTangle\Tables\Generated\EventOutboxData.sql
  [Files: Unchanged = 0, Updated = 0, Created = 1]

Complete. [1288ms, Files: Unchanged = 0, Updated = 0, Created = 21, TotalLines = 1022]
```

<br/>

### FooBar.Database

As stated earlier, the generated artefacts are not included in the project automatically - these all need to be added. Within Visual Studio the _Show All Files_ in the _Solution Explorer_ can be used to view, and then added en masse by selecting and using the _Include In Project_ (context menu) function. The _Build Action_ for `Generated\CdcEnable.sql` will need to be updated to `None` (see [earlier instructions tip](#AppName.Database)).

The artefacts to be added are as follows.

```
└── Generated
  └── CdcEnable.sql
└── Schema
  └── NTangle
    └── Generated
      └── NTangle.sql
    └── Stored Procedures
      └── Generated
        └── spContactBatchComplete.sql
        └── spContactBatchExecute.sql
        └── spEventOutboxDequeue.sql
        └── spEventOutboxEnqueue.sql
    └── Tables
      └── Generated
        └── ContactBatchTracking.sql
        └── EventOutbox.sql
        └── EventOutboxData.sql
        └── VersionTracking.sql
    └── Types
      └── User-Defined Table Types
        └── Generated
          └── udtEventOutboxList.sql
          └── udtVersionTrackingList.sql
```

Once added the generated artefacts need to be deployed to the database. Using the _Publish_ function from the project deploy to the previously created `FooBar` database.

<br/>

### FooBar.Publisher

The `Program.cs` is pre-configured, and the `appsettings.json` has the correct connection string. All of the generated C# artefacts should have been automatically included within the .NET project. Compile the application and execute. Leave it running; use `ctrl-c` to stop once the following test has been performed.

Within your favorite database tool make a change to `[Legacy].[Contact]` table, updating the `Name` column in the first row. Within the next 30 seconds similar console output to the following should be displayed.

```
info: FooBar.Publisher.Services.ContactHostedService[0]
      Service started. Timer first/interval 00:00:05/00:00:05.
info: FooBar.Publisher.Services.OutboxDequeueHostedService[0]
      Service started. Timer first/interval 00:00:05/00:00:05.
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: C:\Users\eric\OneDrive\Source\Visual Studio 2019\FooBar\FooBar.Publisher\bin\Debug\netcoreapp3.1
info: FooBar.Publisher.Data.ContactCdcOrchestrator[0]
      Batch '1': 1 entity operations(s) were found. [MaxQuerySize=50, ContinueWithDataLoss=True, CorrelationId=a57c2994-168a-4875-8bde-bd6354188e88, ExecutionId=04d3958d-d573-4ae3-a8b2-0cb451aacab1, Elapsed=56ms]
info: FooBar.Publisher.Data.ContactCdcOrchestrator[0]
      Batch '1': 1 event(s) were published successfully. [Publisher=OutboxEventPublisher, CorrelationId=a57c2994-168a-4875-8bde-bd6354188e88, ExecutionId=04d3958d-d573-4ae3-a8b2-0cb451aacab1, Elapsed=83ms]
info: FooBar.Publisher.Data.ContactCdcOrchestrator[0]
      Batch '1': Marked as Completed. [CorrelationId=a57c2994-168a-4875-8bde-bd6354188e88, ExecutionId=04d3958d-d573-4ae3-a8b2-0cb451aacab1, Elapsed=19ms]
info: FooBar.Publisher.Services.OutboxDequeueHostedService[0]
      1 event(s) were dequeued. [Elapsed=15ms]
info: NTangle.Events.LoggerEventPublisher[0]
      {
        "specversion": "1.0",
        "type": "legacy.contact.updated",
        "source": "/database/cdc/legacy/contact/1",
        "id": "dc8e1380-5fdf-4497-a6d6-47d3e0c29198",
        "time": "2021-11-17T17:03:44.8217601Z",
        "subject": "legacy.contact",
        "action": "updated",
        "correlationid": "a57c2994-168a-4875-8bde-bd6354188e88",
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
          "etag": "jQz7OsJNb3ztBaIPZg9g5Q=="
        }
      }
info: FooBar.Publisher.Services.OutboxDequeueHostedService[0]
      1 event(s) were published successfully. [Publisher=LoggerEventPublisher, Elapsed=3ms]
```