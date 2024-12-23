CREATE TABLE [NTangle].[CustomerBatchTracking] (
  [BatchTrackingId] BIGINT IDENTITY (1, 1) NOT NULL PRIMARY KEY CLUSTERED ([BatchTrackingId] ASC),
  [CreatedDate] DATETIME2 NOT NULL,
  [IsComplete] BIT NOT NULL,
  [CompletedDate] DATETIME2 NULL,
  [CorrelationId] NVARCHAR(127) NULL,
  [HasDataLoss] BIT NOT NULL,
  [CustomerMinLsn] BINARY(10) NULL,  -- Primary table: '[Legacy].[Cust]'
  [CustomerMaxLsn] BINARY(10) NULL,
  INDEX [IX_NTangle_CustomerBatchTracking] ([IsComplete], [BatchTrackingId] DESC)
);