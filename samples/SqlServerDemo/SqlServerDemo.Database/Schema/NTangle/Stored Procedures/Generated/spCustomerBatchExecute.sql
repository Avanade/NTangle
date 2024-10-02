CREATE OR ALTER PROCEDURE [NTangle].[spCustomerBatchExecute]
  @MaxQuerySize BIGINT = 100,         -- Maximum size of query to limit the number of changes to a manageable batch (performance vs failure trade-off).
  @ContinueWithDataLoss BIT = 0       -- Ignores data loss and continues; versus throwing an error.
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
    DECLARE @CustomerBaseMinLsn BINARY(10), @CustomerMinLsn BINARY(10), @CustomerMaxLsn BINARY(10)
    DECLARE @BatchTrackingId INT

    -- Check if there is already an incomplete batch and attempt to reprocess.
    SELECT
        @CustomerMinLsn = [_batch].[CustomerMinLsn],
        @CustomerMaxLsn = [_batch].[CustomerMaxLsn],
        @BatchTrackingId = [BatchTrackingId]
      FROM [NTangle].[CustomerBatchTracking] AS [_batch]
      WHERE [_batch].[IsComplete] = 0 
      ORDER BY [_batch].[BatchTrackingId]

    -- There should never be more than one incomplete batch.
    IF @@ROWCOUNT > 1
    BEGIN
      ;THROW 56002, 'There are multiple incomplete batches; there should not be more than one incomplete batch at any one time.', 1
    END

    -- Get the latest 'base' minimum.
    SET @CustomerBaseMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Cust');

    -- Where there is no incomplete batch then the next should be created/processed.
    IF (@BatchTrackingId IS NULL)
    BEGIN
      -- Get the last batch processed.
      SELECT TOP 1
          @CustomerMinLsn = [_batch].[CustomerMaxLsn]
        FROM [NTangle].[CustomerBatchTracking] AS [_batch]
        ORDER BY [_batch].[IsComplete] ASC, [_batch].[BatchTrackingId] DESC

      IF (@@ROWCOUNT = 0) -- No previous batch; i.e. is the first time!
      BEGIN
        SET @CustomerMinLsn = @CustomerBaseMinLsn;
      END
      ELSE
      BEGIN
        -- Increment the minimum as the last has already been processed.
        SET @CustomerMinLsn = sys.fn_cdc_increment_lsn(@CustomerMinLsn)
      END

      -- Get the maximum LSN.
      SET @CustomerMaxLsn = sys.fn_cdc_get_max_lsn();

      -- Verify the maximum query size and correct (reset) where applicable.
      IF (@MaxQuerySize IS NULL OR @MaxQuerySize < 1 OR @MaxQuerySize > 10000)
      BEGIN
        SET @MaxQuerySize = 100
      END
    END

    -- The minimum should _not_ be less than the base otherwise we have lost data; either continue with this data loss, or error and stop.
    DECLARE @hasDataLoss BIT = 0

    IF (@CustomerMinLsn < @CustomerBaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @CustomerMinLsn = @CustomerBaseMinLsn END ELSE BEGIN ;THROW 56002, 'Unexpected data loss error for ''Legacy.Cust''; this indicates that the CDC data has probably been cleaned up before being successfully processed.', 1; END END

    -- Find changes on the root table: '[Legacy].[Cust]' - this determines overall operation type: 'create', 'update' or 'delete'.
    CREATE TABLE #_changes ([_Lsn] BINARY(10), [_Op] INT, [CustId] INT)
    DECLARE @hasChanges BIT = 0

    IF (@CustomerMinLsn <= @CustomerMaxLsn)
    BEGIN
      INSERT INTO #_changes
        SELECT TOP (@MaxQuerySize)
            [_cdc].[__$start_lsn] AS [_Lsn],
            [_cdc].[__$operation] AS [_Op],
            [_cdc].[CustId] AS [CustId]
          FROM cdc.fn_cdc_get_all_changes_Legacy_Cust(@CustomerMinLsn, @CustomerMaxLsn, 'all') AS [_cdc]
          WHERE ([_cdc].[is-private] IS NULL OR [_cdc].[is-private] = 0)
          ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @CustomerMinLsn = MIN([_Lsn]), @CustomerMaxLsn = MAX([_Lsn]) FROM #_changes
      END
    END

    -- Create a new batch where not processing an existing.
    IF (@BatchTrackingId IS NULL AND (@hasDataLoss = 1 OR @hasChanges = 1))
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
      IF (@BatchTrackingId IS NOT NULL AND @hasDataLoss = 1)
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

    -- Exit here if there were no changes found.
    IF (@hasChanges = 0)
    BEGIN
      COMMIT TRANSACTION
      RETURN 0
    END

    -- Root table: '[Legacy].[Cust]' - uses LEFT OUTER JOIN's to get the deleted records, as well as any previous 'TrackingHash' value.
    SELECT
        [_chg].[_Op] AS [_OperationType],
        [_chg].[_Lsn] AS [_Lsn],
        [_vt].[Hash] AS [_TrackingHash],
        [_chg].[CustId] AS [Id],
        [c].[CustId] AS [TableKey_Id],
        [c].[Name] AS [Name],
        [c].[Email] AS [Email],
        [c].[is-deleted] AS [IsDeleted],
        [c].[RowVersion] AS [RowVersion],
        CASE WHEN EXISTS (SELECT 1 FROM [Legacy].[Cust] AS [__c] WHERE ([__c].[CustId] = [_chg].[CustId])) THEN CAST (0 AS BIT) ELSE CAST (1 AS BIT) END AS [_IsPhysicallyDeleted]
      FROM #_changes AS [_chg]
      LEFT OUTER JOIN [Legacy].[Cust] AS [c] ON ([c].[CustId] = [_chg].[CustId])
      LEFT OUTER JOIN [NTangle].[VersionTracking] AS [_vt] ON ([_vt].[Schema] = N'Legacy' AND [_vt].[Table] = N'Customer' AND [_vt].[Key] = CAST([_chg].[CustId] AS NVARCHAR(128)))
      ORDER BY [_Lsn] ASC

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