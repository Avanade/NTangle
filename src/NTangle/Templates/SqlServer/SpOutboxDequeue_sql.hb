{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle }}
CREATE PROCEDURE [{{OutboxSchema}}].[sp{{OutboxTable}}Dequeue]
  @MaxDequeueSize INT = 10  -- Maximum number of events to dequeue
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */
 
  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Dequeued outbox resultant identifier.
    DECLARE @dequeuedId TABLE([{{OutboxTable}}Id] BIGINT);

    -- Dequeue event -> ROWLOCK+UPDLOCK maintain singular access for ordering and concurrency
    WITH cte([{{OutboxTable}}Id], [DequeuedDate]) AS 
    (
       SELECT TOP(@MaxDequeueSize) [{{OutboxTable}}Id], [DequeuedDate]
         FROM [{{OutboxSchema}}].[{{OutboxTable}}] WITH (ROWLOCK, UPDLOCK)
         WHERE [DequeuedDate] IS NULL
         ORDER BY [{{OutboxTable}}Id]
    ) 
    UPDATE Cte
      SET [DequeuedDate] = SYSUTCDATETIME()
      OUTPUT deleted.{{OutboxTable}}Id INTO @dequeuedId;

    -- Get the dequeued event outbox data.
    SELECT
        [{{OutboxTable}}DataId] as [{{OutboxTable}}Id],
        [EventId],
        [Type], 
        [Source], 
        [Timestamp], 
        [CorrelationId], 
        [EventData]
      FROM [{{OutboxSchema}}].[{{OutboxTable}}Data]
      WHERE [{{OutboxTable}}DataId] IN (SELECT [{{OutboxTable}}Id] FROM @dequeuedId)

    -- Commit the transaction.
    COMMIT TRANSACTION
    RETURN 0
  END TRY
  BEGIN CATCH
    -- Rollback transaction and rethrow error.
    IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;

    THROW;
  END CATCH
END