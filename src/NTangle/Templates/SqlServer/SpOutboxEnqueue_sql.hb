{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle }}
CREATE PROCEDURE [{{OutboxSchema}}].[sp{{OutboxTable}}Enqueue]
  @EventList AS [{{OutboxSchema}}].[udt{{OutboxTable}}List] READONLY
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */
 
  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Working variables.
    DECLARE @{{camel OutboxTable}}Id BIGINT,
            @enqueuedDate DATETIME

    SET @enqueuedDate = SYSUTCDATETIME()

    -- Enqueued outbox resultant identifier.
    DECLARE @enqueuedId TABLE([{{OutboxTable}}Id] BIGINT)

    -- Cursor output variables.
    DECLARE @eventId NVARCHAR(128),
            @type NVARCHAR(1024),
            @source NVARCHAR(1024),
            @timestamp DATETIMEOFFSET,
            @correlationId NVARCHAR(128),
            @eventData VARBINARY(MAX)

    -- Declare, open, and fetch first event from cursor.
    DECLARE c CURSOR FORWARD_ONLY 
      FOR SELECT [EventId], [Type], [Source], [Timestamp], [CorrelationId], [EventData] FROM @EventList

    OPEN c
    FETCH NEXT FROM c INTO @eventId, @type, @source, @timestamp, @correlationId, @eventdata

    -- Iterate the event(s).
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Enqueue event into outbox 
        INSERT INTO [{{OutboxSchema}}].[{{OutboxTable}}] ([EnqueuedDate])
          OUTPUT inserted.{{OutboxTable}}Id INTO @enqueuedId 
          VALUES (@enqueuedDate)

        SELECT @{{camel OutboxTable}}Id = [{{OutboxTable}}Id] FROM @enqueuedId

        -- Insert corresponding event data.
        INSERT INTO [{{OutboxSchema}}].[{{OutboxTable}}Data] (
          [{{OutboxTable}}DataId],
          [EventId],
          [Type], 
          [Source], 
          [Timestamp], 
          [CorrelationId], 
          [EventData]
        ) 
        VALUES (
          @{{camel OutboxTable}}Id,
          @eventId,
          @type, 
          @source,
          @timestamp, 
          @correlationId, 
          @eventdata
        )

        -- Fetch the next event from the cursor.
        FETCH NEXT FROM c INTO @eventId, @type, @source, @timestamp, @correlationId, @eventdata
    END

    -- Close the cursor.
    CLOSE c
    DEALLOCATE c

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