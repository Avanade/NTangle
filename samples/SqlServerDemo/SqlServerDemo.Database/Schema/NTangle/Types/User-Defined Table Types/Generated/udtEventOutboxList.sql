CREATE TYPE [NTangle].[udtEventOutboxList] AS TABLE (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [EventId] NVARCHAR(128),
  [Type] NVARCHAR(1024) NULL,
  [Source] NVARCHAR(1024) NULL,
  [Timestamp] DATETIMEOFFSET,
  [CorrelationId] NVARCHAR(128),
  [EventData] VARBINARY(MAX) NULL
)