CREATE TABLE [NTangle].[EventOutboxData] (
  [EventOutboxDataId] BIGINT NOT NULL PRIMARY KEY CLUSTERED ([EventOutboxDataId] ASC) CONSTRAINT FK_NTangle_EventOutboxData_EventOutbox FOREIGN KEY REFERENCES [NTangle].[EventOutbox] ([EventOutboxId]) ON DELETE CASCADE,
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
);