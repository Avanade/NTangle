CREATE TYPE [NTangle].[udtIdentifierMappingList] AS TABLE (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [Schema] VARCHAR(50) NOT NULL,
  [Table] VARCHAR(127) NOT NULL,
  [Key] NVARCHAR(255) NOT NULL,
  [GlobalId] NVARCHAR(127) NOT NULL
)