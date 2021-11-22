{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle }}
CREATE TABLE [{{CdcSchema}}].[{{VersionTrackingTable}}] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [{{VersionTrackingTable}}Id] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY NONCLUSTERED ([{{VersionTrackingTable}}Id] ASC),
  [Schema] VARCHAR(64) NOT NULL,
  [Table] VARCHAR(128) NOT NULL,
  [Key] NVARCHAR(128) NOT NULL,
  [Hash] NVARCHAR(128) NOT NULL,
  [BatchTrackingId] BIGINT NOT NULL,
  CONSTRAINT [IX_{{CdcSchema}}_{{VersionTrackingTable}}_SchemaTableKey] UNIQUE CLUSTERED ([Schema], [Table], [Key])
);