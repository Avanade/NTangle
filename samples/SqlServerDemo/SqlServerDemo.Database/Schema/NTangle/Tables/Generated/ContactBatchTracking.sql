CREATE TABLE [NTangle].[ContactBatchTracking] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [BatchTrackingId] BIGINT IDENTITY (1, 1) NOT NULL PRIMARY KEY CLUSTERED ([BatchTrackingId] ASC),
  [CreatedDate] DATETIME2 NOT NULL,
  [IsComplete] BIT NOT NULL,
  [CompletedDate] DATETIME2 NULL,
  [CorrelationId] NVARCHAR(128) NULL,
  [HasDataLoss] BIT NOT NULL,
  [ContactMinLsn] BINARY(10) NULL,  -- Primary table: '[Legacy].[Contact]'
  [ContactMaxLsn] BINARY(10) NULL,
  [AddressMinLsn] BINARY(10) NULL,  -- Related table: '[Legacy].[Address]'
  [AddressMaxLsn] BINARY(10) NULL
);