CREATE PROCEDURE [NTangle].[spContactBatchComplete]
  @BatchTrackingId BIGINT,
  @VersionTrackingList AS [NTangle].[udtVersionTrackingList] READONLY
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
      THROW 56002, @Msg, 1;
    END

    UPDATE [_batch] SET
        [_batch].[IsComplete] = 1,
        [_batch].[CompletedDate] = GETUTCDATE()
      FROM [NTangle].[ContactBatchTracking] AS [_batch]
      WHERE BatchTrackingId = @BatchTrackingId 

    MERGE INTO [NTangle].[VersionTracking] WITH (HOLDLOCK) AS [_vt]
      USING @VersionTrackingList AS [_list] ON ([_vt].[Schema] = N'old' AND [_vt].[Table] = N'Contact' AND [_vt].[Key] = [_list].[Key])
      WHEN MATCHED AND EXISTS (
          SELECT [_list].[Key], [_list].[Hash]
          EXCEPT
          SELECT [_vt].[Key], [_vt].[Hash])
        THEN UPDATE SET [_vt].[Hash] = [_list].[Hash], [_vt].[BatchTrackingId] = @BatchTrackingId
      WHEN NOT MATCHED BY TARGET
        THEN INSERT ([Schema], [Table], [Key], [Hash], [BatchTrackingId])
          VALUES (N'old', N'Contact', [_list].[Key], [_list].[Hash], @BatchTrackingId);

    SELECT [_batch].[BatchTrackingId], [_batch].[CreatedDate], [_batch].[IsComplete], [_batch].[CompletedDate], [_batch].[CorrelationId], [_batch].[HasDataLoss]
      FROM [NTangle].[ContactBatchTracking] AS [_batch]
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