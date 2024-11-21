CREATE OR ALTER PROCEDURE [NTangle].[spCustomerBatchExecute]
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Check if there is already an incomplete batch and attempt to reprocess.
    SELECT TOP 2
        [_batch].[CustomerMinLsn],
        [_batch].[CustomerMaxLsn],
        [_batch].[BatchTrackingId],
        [_batch].[CreatedDate],
        [_batch].[IsComplete],
        [_batch].[CompletedDate],
        [_batch].[CorrelationId],
        [_batch].[HasDataLoss]
      FROM [NTangle].[CustomerBatchTracking] AS [_batch]
      WHERE [_batch].[IsComplete] = 0
      ORDER BY [_batch].[BatchTrackingId]

    -- There should never be more than one incomplete batch.
    IF @@ROWCOUNT > 1
    BEGIN
      ;THROW 56010, 'There are multiple incomplete batches; there should not be more than one incomplete batch at any one time.', 1
    END

    -- Where no incomplete batch exists, then get the last completed batch.
    IF @@ROWCOUNT = 0
    BEGIN
      SELECT TOP 1
          [_batch].[CustomerMinLsn],
          [_batch].[CustomerMaxLsn],
          [_batch].[BatchTrackingId],
          [_batch].[CreatedDate],
          [_batch].[IsComplete],
          [_batch].[CompletedDate],
          [_batch].[CorrelationId],
          [_batch].[HasDataLoss]
        FROM [NTangle].[CustomerBatchTracking] AS [_batch]
        WHERE [_batch].[IsComplete] = 1
        ORDER BY [_batch].[BatchTrackingId] DESC
    END

    RETURN 0
  END TRY
  BEGIN CATCH
    -- Rollback transaction and rethrow error.
    IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;

    THROW;
  END CATCH
END