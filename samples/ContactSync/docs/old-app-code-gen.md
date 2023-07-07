## Step 2 - Old App Code Generation

This will walk through the configuration and execution of the `OldApp`-related code-generation to output both database and .NET artefacts. 

<br/>

## Code generation

To enable the _NTangle_ capabilities the `ContactSync.OldApp.CodeGen` project must be configured and the corresponding code generation performed to create the requisite database and .NET runtime artefacts.

Similar to the previous `ContactSync.OldApp.Database` project, the connection string within the [`Program.cs`](../ContactSync.OldApp/ContactSync.OldApp.CodeGen/Program.cs) file should be reviewed and amended accordingly.

The specified connection string is intended as the default for development only. This value can also be specified at runtime via the command line arguments. To see the available command line arguments etc. issue the following command from the `ContactSync.OldApp.CodeGen` folder.

```
dotnet run -- --help
```

<br/>

### Configuration

The `ntangle.yaml` must be configured to enable the code generation. Replace the existing contents with the following. 

In summary, the configuration defines both the `contact` and `contact_address` tables as being CDC-triggered and are renamed. The relationship between them is defined including the corresponding cardinality of one-to-one. Their corresponding columns have been renamed using an alias to produce the desired .NET naming convention. The global settings are also defined within the YAML. All possible YAML configuration settings are documented [here](../../../docs/generated/root.md).

```yaml
schema: old                                          # Defaults the schema for the non-CDC related tables.
cdcSchema: NTangle                                   # Defines the name of the schema to be used for CDC-related artefacts.
cdcSchemaCreate: true                                # Indicates whether to create the CDC database schema via code-generation.
cdcEnable: true                                      # Indicates whether to enable CDC for the referenced table(s) in the database (recommend managing explicitly).
eventSourceRoot: /database/old/cdc                   # Event source URI root prepended to all published events.
eventSubjectRoot: old                                # Event subject root prepended to all published events.
outbox: true                                         # Indicates to include the generation of the event outbox artefacts.
outboxSchemaCreate: true                             # Indicates whether to create the Outbox database schema via code-generation.
service: Service                                     # Create the entity-based "Service" type to be executed by the likes of an Azure Function.
tables:
  # Set up CDC-entity for primary table old.contact; note rename of table and columns.
- { name: Contact, table: contact, eventSubject: Contact, aliasColumns: [ contact_id^Id, contact_name^Name, contact_phone^Phone, contact_email^Email, contact_active^IsActive, contact_no_calling^NoCallList ], excludeColumns: [ contact_addressid ],
    joins: [
      # Set up secondary CDC-triggered One-To-One relationship (join) from old.contact to old.contact_address renaming the table and columns.
      { name: Address, table: contact_address, joinCardinality: OneToOne, excludeColumns: [ contact_address_id ], aliasColumns: [ contact_address_id^Id, address_street_1^Street1, address_street_2^Street2 ],
        on: [
          { name: contact_address_id, toColumn: contact_addressid }
        ]
      }
    ]
  }
```

<br/>

### Execute code generation

Once the configuration has been completed then the code-generation is performed. Remember that the code-generation is _gen-many_ meaning that at any time the code-generation can be re-executed to create/update the related artefacts; such as schema changes to related tables, or the addition of new tables, etc.

Issue the following command from the `ContactSync.OldApp.CodeGen` folder to execute the code-generation.

```
dotnet run
```

<br/>

## Database management

Return to the `ContactSync.OldApp.Database` project. Within the `Migrations` and `Schema` folders new artefacts will have been created as a result of the preceeding code-generation. These include the _NTangle_ and _Outbox_ related tables and stored procedures. 

Using the inherited _DbEx_ capabilities, the database can be updated to include these new artefacts. Issue the following command from the `ContactSync.OldApp.Database` folder.

```
dotnet run database
```

_Note:_ It is recommended that the `CdcEnable.post.deploy.sql` be reviewed as this is intended for demo purposes only; this should not be used for production/actual use, and that the equivalent script(s) are managed manually to get the appropriate/desired outcome. To turn off the code-generation for this, update the YAML setting to `cdcEnable: false` or remove the configuration altogether.

Finally, the database mamagement capabilities can be leveraged (where applicable) to manage the database changes in the various environments; for example, development, testing and production.

<br/>

## Conclusion

At this stage we have now executed the _NTangle_ code-generation against the `OldApp` database, and output both the database and .NET artefacts. The database has been updated to include the _NTangle_ and _Outbox_ related tables and stored procedures, etc.

Next we will update the _NTangle_ [publisher](./old-app-publish.md) project as the deployed component that leverages CDC to execute the event publish triggering. 
