{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle }}
CREATE TABLE [{{CdcSchema}}].[{{IdentifierMappingTable}}] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [Schema] VARCHAR(64) NOT NULL,
  [Table] VARCHAR(128) NOT NULL,
  [Key] NVARCHAR(128) NOT NULL,
  [GlobalId] {{IdentifierMappingSqlType}} NOT NULL,
  CONSTRAINT [PK_{{CdcSchema}}_{{IdentifierMappingTable}}_SchemaTableKey] PRIMARY KEY CLUSTERED ([Schema], [Table], [Key]),
  CONSTRAINT [IX_{{CdcSchema}}_{{IdentifierMappingTable}}_SchemaTableGlobalId] UNIQUE ([Schema], [Table], [GlobalId])
);