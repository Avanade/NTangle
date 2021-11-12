# 'JoinMapping' object (database-driven)

The `JoinMapping` object defines one or more related table identifier mappings.

<br/>

## Example

A YAML configuration example is as follows:
``` yaml

```

<br/>

## Properties
The `JoinMapping` object supports a number of properties that control the generated code output. The following properties with a bold name are those that are more typically used (considered more important).

Property | Description
-|-
**`name`** | The name of of the existing column that requires identifier mapping. [Mandatory]
`schema` | The schema name of the related table.<br/><br/>Defaults to the owning (parent) table schema.
**`table`** | The name of the related table. [Mandatory]

