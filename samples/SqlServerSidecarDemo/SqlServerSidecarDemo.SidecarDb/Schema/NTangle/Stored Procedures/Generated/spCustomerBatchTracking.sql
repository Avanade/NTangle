CREATE OR ALTER PROCEDURE [NTangle].[spCustomerBatchTracking]
  @BatchTrackingId BIGINT,
  @CustomerMinLsn BINARY(10),
  @CustomerMaxLsn BINARY(10),
  @VersionTrackingList AS NVARCHAR(MAX),
  @HasDataLoss BIT
--  @IsComplete BIT
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Create or updates the batch tracking accordingly.
    IF @BatchTrackingId = 0
    BEGIN
      DECLARE @InsertedBatchTrackingId TABLE([BatchTrackingId] INT)

      INSERT INTO [NTangle].[CustomerBatchTracking] (
          [CustomerMinLsn],
          [CustomerMaxLsn],
          [CreatedDate],
          [IsComplete],
          [CorrelationId],
          [HasDataLoss]
        )
        OUTPUT inserted.BatchTrackingId INTO @InsertedBatchTrackingId
        VALUES (
          @CustomerMinLsn,
          @CustomerMaxLsn,
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
        UPDATE [NTangle].[CustomerBatchTracking]
          SET [HasDataLoss] = @hasDataLoss
          WHERE [BatchTrackingId] = @BatchTrackingId
      END
    END

    -- Return the *latest* batch tracking data.
    SELECT [_batch].[BatchTrackingId], [_batch].[CreatedDate], [_batch].[IsComplete], [_batch].[CompletedDate], [_batch].[CorrelationId], [_batch].[HasDataLoss]
      FROM [NTangle].[CustomerBatchTracking] AS [_batch]
      WHERE [_batch].BatchTrackingId = @BatchTrackingId 

    -- Get the existing version tracking hashes.
    SELECT
        [_vt].[Key],
        [_vt].[Hash]
      FROM [NTangle].[VersionTracking] as [_vt]
      WHERE [_vt].[Schema] = N'Legacy' AND [_vt].[Table] = N'Cust' AND [_vt].[Key] IN (SELECT VALUE FROM OPENJSON(@VersionTrackingList))

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