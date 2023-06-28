CREATE TABLE [NTangle].[ContactBatchTracking] (
  [BatchTrackingId] BIGINT IDENTITY (1, 1) NOT NULL PRIMARY KEY CLUSTERED ([BatchTrackingId] ASC),
  [CreatedDate] DATETIME2 NOT NULL,
  [IsComplete] BIT NOT NULL,
  [CompletedDate] DATETIME2 NULL,
  [CorrelationId] NVARCHAR(127) NULL,
  [HasDataLoss] BIT NOT NULL,
  [ContactMinLsn] BINARY(10) NULL,  -- Primary table: '[old].[contact]'
  [ContactMaxLsn] BINARY(10) NULL,
  [AddressMinLsn] BINARY(10) NULL,  -- Related table: '[old].[contact_address]'
  [AddressMaxLsn] BINARY(10) NULL
);