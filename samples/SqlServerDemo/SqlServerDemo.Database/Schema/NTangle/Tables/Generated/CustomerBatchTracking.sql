CREATE TABLE [NTangle].[CustomerBatchTracking] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [BatchTrackingId] BIGINT IDENTITY (1, 1) NOT NULL PRIMARY KEY CLUSTERED ([BatchTrackingId] ASC),
  [CreatedDate] DATETIME2 NOT NULL,
  [IsComplete] BIT NOT NULL,
  [CompletedDate] DATETIME2 NULL,
  [CorrelationId] NVARCHAR(128) NULL,
  [HasDataLoss] BIT NOT NULL,
  [CustomerMinLsn] BINARY(10) NULL,  -- Primary table: '[Legacy].[Customer]'
  [CustomerMaxLsn] BINARY(10) NULL,
);