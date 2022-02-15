# 'JoinOn' object (database-driven)

The `JoinOn` object defines the join on characteristics for a `Join` object.

<br/>

## Properties
The `JoinOn` object supports a number of properties that control the generated code output. The following properties with a bold name are those that are more typically used (considered more important).

Property | Description
-|-
**`name`** | The name of the join column (from the `Join` table). [Mandatory]
**`toColumn`** | The name of the join to column.<br/>&dagger; Defaults to `Name`; i.e. assumes same name.
`toStatement` | The SQL statement for the join on bypassing the corresponding `ToColumn` specification.

