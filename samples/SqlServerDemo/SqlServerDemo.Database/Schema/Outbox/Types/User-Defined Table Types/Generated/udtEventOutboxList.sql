CREATE TYPE [Outbox].[udtEventOutboxList] AS TABLE (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [EventId] NVARCHAR(127),
  [Destination] NVARCHAR(127) NULL,
  [Subject] NVARCHAR(511) NULL,
  [Action] NVARCHAR(255) NULL,
  [Type] NVARCHAR(1023) NULL,
  [Source] NVARCHAR(1023) NULL,
  [Timestamp] DATETIMEOFFSET,
  [CorrelationId] NVARCHAR(127),
  [TenantId] NVARCHAR(127),
  [PartitionKey] NVARCHAR(127),
  [ETag] NVARCHAR(127),
  [Attributes] VARBINARY(MAX) NULL,
  [Data] VARBINARY(MAX) NULL
)