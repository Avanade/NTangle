{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle }}
CREATE TABLE [{{CdcSchema}}].[{{BatchTrackingTable}}] (
{{#unless Root.IsGenOnce}}
  /*
   * This is automatically generated; any changes will be lost.
   */

{{/unless}}
  [BatchTrackingId] BIGINT IDENTITY (1, 1) NOT NULL PRIMARY KEY CLUSTERED ([BatchTrackingId] ASC),
  [CreatedDate] DATETIME2 NOT NULL,
  [IsComplete] BIT NOT NULL,
  [CompletedDate] DATETIME2 NULL,
  [CorrelationId] NVARCHAR(128) NULL,
  [HasDataLoss] BIT NOT NULL,
  [{{pascal Name}}MinLsn] BINARY(10) NULL,  -- Primary table: '[{{Schema}}].[{{Table}}]'
  [{{pascal Name}}MaxLsn] BINARY(10) NULL,
{{#each CdcJoins}}
  [{{pascal Name}}MinLsn] BINARY(10) NULL,  -- Related table: '[{{Schema}}].[{{Table}}]'
  [{{pascal Name}}MaxLsn] BINARY(10) NULL{{#unless @last}},{{/unless}}
{{/each}}
);