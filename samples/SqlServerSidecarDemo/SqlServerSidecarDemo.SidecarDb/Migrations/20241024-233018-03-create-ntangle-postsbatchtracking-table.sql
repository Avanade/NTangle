CREATE TABLE [NTangle].[PostsBatchTracking] (
  [BatchTrackingId] BIGINT IDENTITY (1, 1) NOT NULL PRIMARY KEY CLUSTERED ([BatchTrackingId] ASC),
  [CreatedDate] DATETIME2 NOT NULL,
  [IsComplete] BIT NOT NULL,
  [CompletedDate] DATETIME2 NULL,
  [CorrelationId] NVARCHAR(127) NULL,
  [HasDataLoss] BIT NOT NULL,
  [PostsMinLsn] BINARY(10) NULL,  -- Primary table: '[Legacy].[Posts]'
  [PostsMaxLsn] BINARY(10) NULL,
  [CommentsMinLsn] BINARY(10) NULL,  -- Related table: '[Legacy].[Comments]'
  [CommentsMaxLsn] BINARY(10) NULL,
  [CommentsTagsMinLsn] BINARY(10) NULL,  -- Related table: '[Legacy].[Tags]'
  [CommentsTagsMaxLsn] BINARY(10) NULL,
  [PostsTagsMinLsn] BINARY(10) NULL,  -- Related table: '[Legacy].[Tags]'
  [PostsTagsMaxLsn] BINARY(10) NULL,
  INDEX [IX_NTangle_PostsBatchTracking] ([IsComplete], [BatchTrackingId] DESC)
);