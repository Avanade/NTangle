CREATE OR ALTER PROCEDURE [NTangle].[spPostsBatchTracking]
  @BatchTrackingId BIGINT NULL = NULL,
  @PostsMinLsn BINARY(10) NULL = NULL,
  @PostsMaxLsn BINARY(10) NULL = NULL,
  @CommentsMinLsn BINARY(10) NULL = NULL,
  @CommentsMaxLsn BINARY(10) NULL = NULL,
  @CommentsTagsMinLsn BINARY(10) NULL = NULL,
  @CommentsTagsMaxLsn BINARY(10) NULL = NULL,
  @PostsTagsMinLsn BINARY(10) NULL = NULL,
  @PostsTagsMaxLsn BINARY(10) NULL = NULL,
  @VersionTrackingList AS NVARCHAR(MAX),
  @HasDataLoss BIT = 0
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Create or update the batch tracking accordingly (where applicable).
    IF @BatchTrackingId IS NOT NULL
    BEGIN
      IF @BatchTrackingId = 0
      BEGIN
        DECLARE @InsertedBatchTrackingId TABLE([BatchTrackingId] INT)
  
        INSERT INTO [NTangle].[PostsBatchTracking] (
            [PostsMinLsn],
            [PostsMaxLsn],
            [CommentsMinLsn],
            [CommentsMaxLsn],
            [CommentsTagsMinLsn],
            [CommentsTagsMaxLsn],
            [PostsTagsMinLsn],
            [PostsTagsMaxLsn],
            [CreatedDate],
            [IsComplete],
            [CorrelationId],
            [HasDataLoss]
          )
          OUTPUT inserted.BatchTrackingId INTO @InsertedBatchTrackingId
          VALUES (
            @PostsMinLsn,
            @PostsMaxLsn,
            @CommentsMinLsn,
            @CommentsMaxLsn,
            @CommentsTagsMinLsn,
            @CommentsTagsMaxLsn,
            @PostsTagsMinLsn,
            @PostsTagsMaxLsn,
            GETUTCDATE(),
            0,
            LOWER(CONVERT(NVARCHAR(64), NEWID())),
            @hasDataLoss
          )
  
          SELECT @BatchTrackingId = [BatchTrackingId] FROM @InsertedBatchTrackingId
      END
      ELSE
      BEGIN
        IF (@hasDataLoss = 1)
        BEGIN
          UPDATE [NTangle].[PostsBatchTracking]
            SET [HasDataLoss] = @hasDataLoss
            WHERE [BatchTrackingId] = @BatchTrackingId
        END
      END
  
      -- Return the *latest* batch tracking data.
      SELECT [_batch].[BatchTrackingId], [_batch].[CreatedDate], [_batch].[IsComplete], [_batch].[CompletedDate], [_batch].[CorrelationId], [_batch].[HasDataLoss]
        FROM [NTangle].[PostsBatchTracking] AS [_batch]
        WHERE [_batch].BatchTrackingId = @BatchTrackingId 
    END

    -- Get the existing version tracking hashes.
    SELECT
        [_vt].[Key],
        [_vt].[Hash]
      FROM [NTangle].[VersionTracking] as [_vt]
      WHERE [_vt].[Schema] = N'Legacy' AND [_vt].[Table] = N'Posts' AND [_vt].[Key] IN (SELECT VALUE FROM OPENJSON(@VersionTrackingList))

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