## Step 1 - Old App Setup

This will walk through the setup of the `OldApp` solution. 

In reality, given `OldApp` is meant to be an existing, legacy application, the application iself should be pre-existing. The technology used to implement the old application is not important, the only constraint is that the application _must_ be leveraging a Microsoft SQL Server database.

<br/>

## Solution skeleton

This solution should be created using the [_NTangle_ template](../../../tools/NTangle.Template/README.md). Additionally, the intent is to leverage [_DbEx_](https://github.com/Avanade/dbex) to provide the database management capabilities, and the underlying publisher capability should be implemented as an [Azure Function](https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview).

The following six commands should be invoked to create the solution structure. Start in the root folder where the solution should be created. To simplify the ongoing copy and paste activities within this sample it is _highly recommended_ that the `ContactSync` and `OldApp` naming convention is used.

```
dotnet new install ntangle.template --nuget-source https://api.nuget.org/v3/index.json
mkdir ContactSync
cd ContactSync
mkdir ContactSync.OldApp
cd ContactSync.OldApp
dotnet new ntangle --publisher Function
```

The following solution structure will have been generated. Open `ContactSync.OldApp.sln` in Visual Studio from within the `ContactSync.OldApp` folder.

```
└── ContactSync
  └── ContactSync.OldApp
    └── ContactSync.OldApp.CodeGen     # NTangle code generation project
    └── ContactSync.OldApp.Database    # Database project leveraging DbEx
    └── ContactSync.OldApp.Publisher   # Publisher Azure Function project
```

<br/>

## Database baseline

At this stage, the only artefact within the `ContactSync.OldApp.Database` project should be the [`Program.cs`](../ContactSync.OldApp/ContactSync.OldApp.Database/Program.cs). Review the included connection string and amend accordingly.

The specified connection string is intended as the default for development only. This value can also be specified at runtime via the command line arguments. To see the available command line arguments etc. issue the following command from the `ContactSync.OldApp.Database` folder.

```
dotnet run -- --help
```

For the purposes of this sample the `OldApp` database must exist, and this database must contain the `contact` and related `contact_address` tables housed within the `old` schema. This can be managed by copying the contents from the sample [`Setup.post.database.create.sql`](../ContactSync.OldApp/ContactSync.OldApp.Database/Migrations/Setup.post.database.create.sql) to the same named file under the `Migrations` folder. Because of the `*.post.database.create.sql` naming convention `DbEx` will only executed this script when the database is being created; i.e. where executing against a pre-existing database this script will not be executed. This script also contains the command to enable CDC within the database.

To create the database, issue the following command from the `ContactSync.OldApp.Database` folder. Post execution the database should exist, the two tables should exist, and internally CDC should be enabled.

```
dotnet run create
```

<br/>

## Conclusion

At this stage we have now setup the `OldApp` solution structure, and for the purposes of this sample have created the database baseline to simulate existing capabilitites.

Next we will configure and execute the _NTangle_ [code-generation](./old-app-code-gen.md) to output the required database and .NET artefacts.