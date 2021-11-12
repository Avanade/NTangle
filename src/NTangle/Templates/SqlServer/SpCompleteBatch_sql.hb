{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle }}
CREATE PROCEDURE [{{CdcSchema}}].[{{CompleteStoredProcedure}}]
  @BatchTrackingId BIGINT,
  @VersionTrackingList AS [{{Root.CdcSchema}}].[udt{{Root.VersionTrackingTable}}List] READONLY
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
      FROM [{{CdcSchema}}].[{{BatchTrackingTable}}] AS [_batch]
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
      FROM [{{CdcSchema}}].[{{BatchTrackingTable}}] AS [_batch]
      WHERE BatchTrackingId = @BatchTrackingId 

    MERGE INTO [{{Root.CdcSchema}}].[{{Root.VersionTrackingTable}}] WITH (HOLDLOCK) AS [_ct]
      USING @VersionTrackingList AS [_list] ON ([_ct].[Schema] = '{{Schema}}' AND [_ct].[Table] = '{{Table}}' AND [_ct].[Key] = [_list].[Key])
      WHEN MATCHED AND EXISTS (
          SELECT [_list].[Key], [_list].[Hash]
          EXCEPT
          SELECT [_ct].[Key], [_ct].[Hash])
        THEN UPDATE SET [_ct].[Hash] = [_list].[Hash], [_ct].[BatchTrackingId] = @BatchTrackingId
      WHEN NOT MATCHED BY TARGET
        THEN INSERT ([Schema], [Table], [Key], [Hash], [BatchTrackingId])
          VALUES ('{{Schema}}', '{{Table}}', [_list].[Key], [_list].[Hash], @BatchTrackingId);

    SELECT [_batch].[BatchTrackingId], [_batch].[CreatedDate], [_batch].[IsComplete], [_batch].[CompletedDate], [_batch].[CorrelationId], [_batch].[HasDataLoss]
      FROM [{{CdcSchema}}].[{{BatchTrackingTable}}] AS [_batch]
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