CREATE TYPE [NTangle].[udtIdentifierMappingList] AS TABLE (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [Schema] VARCHAR(50) NOT NULL,
  [Table] VARCHAR(128) NOT NULL,
  [Key] NVARCHAR(256) NOT NULL,
  [GlobalId] NVARCHAR(128) NOT NULL
)