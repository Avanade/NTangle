CREATE TABLE [NTangle].[IdentifierMapping] (
  [Schema] VARCHAR(63) NOT NULL,
  [Table] VARCHAR(127) NOT NULL,
  [Key] NVARCHAR(255) NOT NULL,
  [GlobalId] NVARCHAR(127) NOT NULL,
  CONSTRAINT [PK_NTangle_IdentifierMapping_SchemaTableKey] PRIMARY KEY CLUSTERED ([Schema], [Table], [Key]),
  CONSTRAINT [IX_NTangle_IdentifierMapping_SchemaTableGlobalId] UNIQUE ([Schema], [Table], [GlobalId])
);