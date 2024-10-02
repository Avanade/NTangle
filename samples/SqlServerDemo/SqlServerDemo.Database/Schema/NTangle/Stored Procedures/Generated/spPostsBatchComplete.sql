CREATE OR ALTER PROCEDURE [NTangle].[spPostsBatchComplete]
  @BatchTrackingId BIGINT,
  @VersionTrackingList AS NVARCHAR(MAX)
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */
 
  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Mark the batch as complete and merge the version tracking info; then return the updated batch tracking data and stop!
    DECLARE @IsCompleteAlready BIT
    SELECT @IsCompleteAlready = [_batch].[IsComplete]
      FROM [NTangle].[PostsBatchTracking] AS [_batch]
      WHERE BatchTrackingId = @BatchTrackingId 

    DECLARE @Msg NVARCHAR(256)
    IF @@ROWCOUNT <> 1
    BEGIN
      SET @Msg = CONCAT('Batch ''', @BatchTrackingId, ''' cannot be completed as it does not exist.');
      THROW 56005, @Msg, 1;
    END

    IF @IsCompleteAlready = 1
    BEGIN
      SET @Msg = CONCAT('Batch ''', @BatchTrackingId, ''' is already complete; cannot be completed more than once.');
      THROW 56002, @Msg, 1;
    END

    DECLARE @Timestamp DATETIME2
    SET @Timestamp = GETUTCDATE()

    SELECT * into #versionTrackingList FROM OPENJSON(@VersionTrackingList) WITH ([Key] NVARCHAR(255) '$.key', [Hash] NVARCHAR(127) '$.hash')

    UPDATE [_batch] SET
        [_batch].[IsComplete] = 1,
        [_batch].[CompletedDate] = @Timestamp
      FROM [NTangle].[PostsBatchTracking] AS [_batch]
      WHERE BatchTrackingId = @BatchTrackingId 

    MERGE INTO [NTangle].[VersionTracking] WITH (HOLDLOCK) AS [_vt]
      USING #versionTrackingList AS [_list] ON ([_vt].[Schema] = N'Legacy' AND [_vt].[Table] = N'Posts' AND [_vt].[Key] = [_list].[Key])
      WHEN MATCHED AND EXISTS (
          SELECT [_list].[Key], [_list].[Hash]
          EXCEPT
          SELECT [_vt].[Key], [_vt].[Hash])
        THEN UPDATE SET [_vt].[Hash] = [_list].[Hash], [_vt].[Timestamp] = @Timestamp, [_vt].[BatchTrackingId] = @BatchTrackingId
      WHEN NOT MATCHED BY TARGET
        THEN INSERT ([Schema], [Table], [Key], [Hash], [Timestamp], [BatchTrackingId])
          VALUES (N'Legacy', N'Posts', [_list].[Key], [_list].[Hash], @Timestamp, @BatchTrackingId);

    SELECT [_batch].[BatchTrackingId], [_batch].[CreatedDate], [_batch].[IsComplete], [_batch].[CompletedDate], [_batch].[CorrelationId], [_batch].[HasDataLoss]
      FROM [NTangle].[PostsBatchTracking] AS [_batch]
      WHERE [_batch].BatchTrackingId = @BatchTrackingId

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