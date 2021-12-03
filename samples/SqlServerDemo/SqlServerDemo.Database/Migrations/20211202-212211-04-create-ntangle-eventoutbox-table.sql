CREATE TABLE [NTangle].[EventOutbox] (
  [EventOutboxId] BIGINT IDENTITY (1, 1) NOT NULL PRIMARY KEY NONCLUSTERED ([EventOutboxId] ASC),
  [EnqueuedDate] DATETIME2 NOT NULL,
  [DequeuedDate] DATETIME2 NULL,
  CONSTRAINT [IX_NTangle_EventOutbox_DequeuedDate] UNIQUE CLUSTERED ([DequeuedDate], [EventOutboxId])
);