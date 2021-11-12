# 'TableMapping' object (database-driven)

The `TableMapping` object defines one or more related table identifier mappings.

<br/>

## Properties
The `TableMapping` object supports a number of properties that control the generated code output. The following properties with a bold name are those that are more typically used (considered more important).

Property | Description
-|-
**`name`** | The name of of the existing column that requires identifier mapping. [Mandatory]
`schema` | The schema name of the related table.<br/>&dagger; Defaults to the owning (parent) table schema.
**`table`** | The name of the related table. [Mandatory]

