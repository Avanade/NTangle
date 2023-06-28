CREATE TABLE [NTangle].[VersionTracking] (
  [VersionTrackingId] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY NONCLUSTERED ([VersionTrackingId] ASC),
  [Schema] VARCHAR(63) NOT NULL,
  [Table] VARCHAR(127) NOT NULL,
  [Key] NVARCHAR(255) NOT NULL,
  [Hash] NVARCHAR(127) NOT NULL,
  [BatchTrackingId] BIGINT NOT NULL,
  CONSTRAINT [IX_NTangle_VersionTracking_SchemaTableKey] UNIQUE CLUSTERED ([Schema], [Table], [Key])
);