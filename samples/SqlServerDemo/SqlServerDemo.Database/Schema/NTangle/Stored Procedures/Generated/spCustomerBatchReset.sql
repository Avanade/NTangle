CREATE PROCEDURE [NTangle].[spCustomerBatchReset]
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Get the maximum lsn.
    DECLARE @MaxLsn BINARY(10)
    SET @MaxLsn = sys.fn_cdc_get_max_lsn();

    -- Complete any incomplete batch (with data loss) with max lsn.
    UPDATE [_batch] SET
        [_batch].[CustomerMaxLsn] = @MaxLsn,
        [_batch].[IsComplete] = 1,
        [_batch].[CompletedDate] = GETUTCDATE(),
        [_batch].[HasDataLoss] = 1
      FROM [NTangle].[CustomerBatchTracking] AS [_batch]
      WHERE [_batch].[IsComplete] = 0;

    -- Create a new batch (with data loss) with max lsn.
    IF @@ROWCOUNT = 0
    BEGIN
      INSERT INTO [NTangle].[CustomerBatchTracking] (
          [CustomerMinLsn],
          [CustomerMaxLsn],
          [CreatedDate],
          [IsComplete],
          [CorrelationId],
          [HasDataLoss]
        )
        VALUES (
          @MaxLsn,
          @MaxLsn,
          GETUTCDATE(),
          1,
          LOWER(CONVERT(NVARCHAR(64), NEWID())),
          1
        )
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