CREATE TABLE [Outbox].[EventOutbox] (
  [EventOutboxId] BIGINT IDENTITY (1, 1) NOT NULL PRIMARY KEY,
  [PartitionKey] NVARCHAR(127) NULL,
  [Destination] NVARCHAR(127) NULL,
  [EnqueuedDate] DATETIME2 NOT NULL,
  [DequeuedDate] DATETIME2 NULL,
  INDEX [IX_Outbox_EventOutbox_DequeuedDate] ([DequeuedDate], [EventOutboxId])
);