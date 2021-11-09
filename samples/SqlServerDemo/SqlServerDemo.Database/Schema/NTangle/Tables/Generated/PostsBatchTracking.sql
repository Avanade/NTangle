CREATE TABLE [NTangle].[PostsBatchTracking] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [BatchTrackingId] BIGINT IDENTITY (1, 1) NOT NULL PRIMARY KEY CLUSTERED ([BatchTrackingId] ASC),
  [CreatedDate] DATETIME2 NOT NULL,
  [IsComplete] BIT NOT NULL,
  [CompletedDate] DATETIME2 NULL,
  [CorrelationId] NVARCHAR(128) NULL,
  [HasDataLoss] BIT NOT NULL,
  [PostsMinLsn] BINARY(10) NULL,  -- Primary table: '[Legacy].[Posts]'
  [PostsMaxLsn] BINARY(10) NULL,
  [CommentsMinLsn] BINARY(10) NULL,  -- Related table: '[Legacy].[Comments]'
  [CommentsMaxLsn] BINARY(10) NULL,
  [CommentsTagsMinLsn] BINARY(10) NULL,  -- Related table: '[Legacy].[Tags]'
  [CommentsTagsMaxLsn] BINARY(10) NULL,
  [PostsTagsMinLsn] BINARY(10) NULL,  -- Related table: '[Legacy].[Tags]'
  [PostsTagsMaxLsn] BINARY(10) NULL
);