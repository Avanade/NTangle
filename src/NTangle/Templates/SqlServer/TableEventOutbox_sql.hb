{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle }}
CREATE TABLE [{{OutboxSchema}}].[{{OutboxTable}}] (
{{#unless Root.IsGenOnce}}
  /*
   * This is automatically generated; any changes will be lost.
   */

{{/unless}}
  [{{OutboxTable}}Id] BIGINT IDENTITY (1, 1) NOT NULL PRIMARY KEY NONCLUSTERED ([{{OutboxTable}}Id] ASC),
  [EnqueuedDate] DATETIME2 NOT NULL,
  [DequeuedDate] DATETIME2 NULL,
  CONSTRAINT [IX_{{OutboxSchema}}_{{OutboxTable}}_DequeuedDate] UNIQUE CLUSTERED ([DequeuedDate], [{{OutboxTable}}Id])
);