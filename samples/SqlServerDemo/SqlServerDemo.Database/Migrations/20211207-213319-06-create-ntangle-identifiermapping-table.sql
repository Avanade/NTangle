CREATE TABLE [NTangle].[IdentifierMapping] (
  [Schema] VARCHAR(64) NOT NULL,
  [Table] VARCHAR(128) NOT NULL,
  [Key] NVARCHAR(128) NOT NULL,
  [GlobalId] NVARCHAR(128) NOT NULL,
  CONSTRAINT [PK_NTangle_IdentifierMapping_SchemaTableKey] PRIMARY KEY CLUSTERED ([Schema], [Table], [Key]),
  CONSTRAINT [IX_NTangle_IdentifierMapping_SchemaTableGlobalId] UNIQUE ([Schema], [Table], [GlobalId])
);