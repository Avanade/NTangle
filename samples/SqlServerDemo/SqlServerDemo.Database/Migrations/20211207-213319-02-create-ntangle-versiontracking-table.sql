CREATE TABLE [NTangle].[VersionTracking] (
  [VersionTrackingId] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY NONCLUSTERED ([VersionTrackingId] ASC),
  [Schema] VARCHAR(64) NOT NULL,
  [Table] VARCHAR(128) NOT NULL,
  [Key] NVARCHAR(128) NOT NULL,
  [Hash] NVARCHAR(128) NOT NULL,
  [BatchTrackingId] BIGINT NOT NULL,
  CONSTRAINT [IX_NTangle_VersionTracking_SchemaTableKey] UNIQUE CLUSTERED ([Schema], [Table], [Key])
);