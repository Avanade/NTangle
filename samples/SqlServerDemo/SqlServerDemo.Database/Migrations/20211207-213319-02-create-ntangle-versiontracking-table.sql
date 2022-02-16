CREATE TABLE [NTangle].[VersionTracking] (
  [VersionTrackingId] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY NONCLUSTERED ([VersionTrackingId] ASC),
  [Object] NVARCHAR(192) NOT NULL,
  [Key] NVARCHAR(256) NOT NULL,
  [Hash] NVARCHAR(128) NOT NULL,
  [BatchTrackingId] BIGINT NOT NULL,
  CONSTRAINT [IX_NTangle_VersionTracking_SchemaTableKey] UNIQUE CLUSTERED ([Object], [Key])
);