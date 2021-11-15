CREATE TABLE [NTangle].[EventOutboxData] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [EventOutboxDataId] BIGINT NOT NULL PRIMARY KEY CLUSTERED ([EventOutboxDataId] ASC) CONSTRAINT FK_NTangle_EventOutboxData_EventOutbox FOREIGN KEY REFERENCES [NTangle].[EventOutbox] ([EventOutboxId]) ON DELETE CASCADE,
  [EventId] NVARCHAR(128),
  [Type] NVARCHAR(1024) NULL,
  [Source] NVARCHAR(1024) NULL,
  [Timestamp] DATETIMEOFFSET,
  [CorrelationId] NVARCHAR(128),
  [EventData] VARBINARY(MAX) NULL
);