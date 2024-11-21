CREATE OR ALTER PROCEDURE [NTangle].[spContactBatchComplete]
  @BatchTrackingId BIGINT NULL,
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

    -- Declare variables.
    DECLARE @Timestamp DATETIME2
    SET @Timestamp = GETUTCDATE()
  
    DECLARE @BatchTrackingId2 BIGINT
    IF @BatchTrackingId IS NULL BEGIN SET @BatchTrackingId2 = 0 END ELSE BEGIN SET @BatchTrackingId2 = @BatchTrackingId END

    -- Update the batch tracking accordingly (where applicable).
    IF @BatchTrackingId IS NOT NULL
    BEGIN
      -- Mark the batch as complete and merge the version tracking info; then return the updated batch tracking data and stop!
      DECLARE @IsCompleteAlready BIT
      SELECT @IsCompleteAlready = [_batch].[IsComplete]
        FROM [NTangle].[ContactBatchTracking] AS [_batch]
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
        THROW 56010, @Msg, 1;
      END
  
      UPDATE [_batch] SET
          [_batch].[IsComplete] = 1,
          [_batch].[CompletedDate] = @Timestamp
        FROM [NTangle].[ContactBatchTracking] AS [_batch]
        WHERE BatchTrackingId = @BatchTrackingId 
    END

    -- Merge the version tracking info.
    SELECT * into #versionTrackingList FROM OPENJSON(@VersionTrackingList) WITH ([Key] NVARCHAR(255) '$.key', [Hash] NVARCHAR(127) '$.hash')

    MERGE INTO [NTangle].[VersionTracking] WITH (HOLDLOCK) AS [_vt]
      USING #versionTrackingList AS [_list] ON ([_vt].[Schema] = N'Legacy' AND [_vt].[Table] = N'Contact' AND [_vt].[Key] = [_list].[Key])
      WHEN MATCHED AND EXISTS (
          SELECT [_list].[Key], [_list].[Hash]
          EXCEPT
          SELECT [_vt].[Key], [_vt].[Hash])
        THEN UPDATE SET [_vt].[Hash] = [_list].[Hash], [_vt].[Timestamp] = @Timestamp, [_vt].[BatchTrackingId] = @BatchTrackingId2
      WHEN NOT MATCHED BY TARGET
        THEN INSERT ([Schema], [Table], [Key], [Hash], [Timestamp], [BatchTrackingId])
          VALUES (N'Legacy', N'Contact', [_list].[Key], [_list].[Hash], @Timestamp, @BatchTrackingId2);

    IF @BatchTrackingId IS NOT NULL
    BEGIN
      SELECT [_batch].[BatchTrackingId], [_batch].[CreatedDate], [_batch].[IsComplete], [_batch].[CompletedDate], [_batch].[CorrelationId], [_batch].[HasDataLoss]
        FROM [NTangle].[ContactBatchTracking] AS [_batch]
        WHERE [_batch].BatchTrackingId = @BatchTrackingId2
    END

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