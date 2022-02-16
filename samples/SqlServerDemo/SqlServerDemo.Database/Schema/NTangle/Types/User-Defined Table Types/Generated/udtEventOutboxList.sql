CREATE TYPE [NTangle].[udtEventOutboxList] AS TABLE (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [EventId] NVARCHAR(128),
  [Subject] NVARCHAR(512) NULL,
  [Action] NVARCHAR(256) NULL,
  [Type] NVARCHAR(1024) NULL,
  [Source] NVARCHAR(1024) NULL,
  [Timestamp] DATETIMEOFFSET,
  [CorrelationId] NVARCHAR(128),
  [TenantId] NVARCHAR(128),
  [PartitionKey] NVARCHAR(128),
  [Data] VARBINARY(MAX) NULL
)