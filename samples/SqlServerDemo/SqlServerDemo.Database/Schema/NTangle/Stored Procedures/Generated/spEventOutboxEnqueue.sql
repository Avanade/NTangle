CREATE PROCEDURE [NTangle].[spEventOutboxEnqueue]
  @EventList AS [NTangle].[udtEventOutboxList] READONLY
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
    DECLARE @eventOutboxId BIGINT,
            @enqueuedDate DATETIME

    SET @enqueuedDate = SYSUTCDATETIME()

    -- Enqueued outbox resultant identifier.
    DECLARE @enqueuedId TABLE([EventOutboxId] BIGINT)

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
        INSERT INTO [NTangle].[EventOutbox] ([EnqueuedDate])
          OUTPUT inserted.EventOutboxId INTO @enqueuedId 
          VALUES (@enqueuedDate)

        SELECT @eventOutboxId = [EventOutboxId] FROM @enqueuedId

        -- Insert corresponding event data.
        INSERT INTO [NTangle].[EventOutboxData] (
          [EventOutboxDataId],
          [EventId],
          [Type], 
          [Source], 
          [Timestamp], 
          [CorrelationId], 
          [EventData]
        ) 
        VALUES (
          @eventOutboxId,
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