# 'Where' object (database-driven)

This should only be used where the column value is largely immutable; otherwise, unintended side-effects may occur. _NTangle_ uses the condition explictily and does not attempt to handle value change to infer creation or deletion of data as a result of the underlying change; as such, this should be used cautiously. Note that the `where` is applied when querying the `cdc.fn_cdc_get_all_changes_...` function, not the underlying table itself.

<br/>

## Properties
The `Where` object supports a number of properties that control the generated code output. The following properties with a bold name are those that are more typically used (considered more important).

Property | Description
-|-
**`name`** | The column name. [Mandatory]<br/>&dagger; The column name.
**`nullable`** | The where nullability clause operator Valid options are: `ISNULL`, `ISNOTNULL`.<br/>&dagger; This enables statements such as `WHERE (COL IS NULL)` or `WHERE (COL IS NULL OR COL = VALUE)` (where .
**`operator`** | The where clause equality operator Valid options are: `EQ`, `NE`, `LT`, `LE`, `GT`, `GE`, `LIKE`.<br/>&dagger; Defaults to `EQ` where `Value` is specified.
`value` | The comparison value<br/>&dagger; This must be valid formatted/escaped SQL.

